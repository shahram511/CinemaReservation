using CinemaReservation.Core.DTOs.Anlaytics;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Infrastructure.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsRepository(ApplicationDbContext context)
        {
           _context = context;
        }       

        public async Task<IEnumerable<ShowtimeCapacityDto>> GetShowtimeCapacitiesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Showtimes.AsNoTracking().AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(s => s.StartTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(s => s.StartTime <= toDate.Value);

            // We  project directly into the DTO to avoid fetching unnecessary columns
            var capacities = await query.Select(s => new ShowtimeCapacityDto()
            {
                ShowtimeId = s.Id,
                MovieTitle = s.Movie.Title,
                StartTime = s.StartTime,
                TotalSeats = _context.Seats.Count(), //Assuming all 60 pyisical seats apply to every showtime
                ReservedSeats = s.Reservations                    
                    .SelectMany(r => r.ReservationSeats)
                    .Where(s => s.Status == Core.Enums.Enums.ReservationStatus.Confirmed)
                    .Count()
            }).ToListAsync();

            foreach (var capacity in capacities)
            {
                //Substract reserved seats from totalseats
                capacity.AvailableSeats = capacity.TotalSeats - capacity.ReservedSeats;

                //calculate the percentage (preventing a divide-by-zero error
                capacity.OccupancyRatePercentage = capacity.TotalSeats == 0
                    ? 
                    0 : Math.Round((double)capacity.ReservedSeats / capacity.TotalSeats * 100, 2);
            }

            return capacities;
        }


        public async Task<IEnumerable<MovieRevenueDto>> GetMovieRevenuesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Movies.AsNoTracking().AsQueryable();

            var revenues = await query.Select(m => new MovieRevenueDto()
            {
                MovieId = m.Id,
                MovieTitle = m.Title,
                TotalTicketSold= m.Showtimes
                    .Where(s => (!fromDate.HasValue || s.StartTime>= fromDate.Value) && (!toDate.HasValue || s.StartTime <= toDate.Value))
                    .SelectMany(s => s.Reservations)                    
                    .SelectMany(r => r.ReservationSeats)
                    .Where(r => r.Status == Core.Enums.Enums.ReservationStatus.Confirmed)
                    .Count(),
                TotalRevenue = m.Showtimes
                    .Where(s  => (!fromDate.HasValue || s.StartTime >=  fromDate.Value) && (!toDate.HasValue || s.StartTime <=toDate.Value))
                    .SelectMany(s  =>  s.Reservations)                                    
                    .SelectMany(s => s.ReservationSeats)
                    .Where(r => r.Status == Core.Enums.Enums.ReservationStatus.Confirmed)
                    .Sum(rs => rs.Price)                  
            }).ToListAsync();

            return revenues;
        }

        public async Task<IEnumerable<TopCustomersDto>> GetTopcuctomersAsync(int count)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            var topCustomers = await query.Select(m => new TopCustomersDto()
            {
                UserId = m.Id,
                Email = m.Email,
                TotalTicketsPurchased = m.Reservations                    
                    .SelectMany(r => r.ReservationSeats)
                    .Where(r => r.Status == Core.Enums.Enums.ReservationStatus.Confirmed)
                    .Count(),
                LifeTimeValue = m.Reservations                    
                    .SelectMany(r => r.ReservationSeats)
                    .Where(r => r.Status == Core.Enums.Enums.ReservationStatus.Confirmed)
                    .Sum(rs => rs.Price),
            }).OrderByDescending(s => s.LifeTimeValue).Take(count).ToListAsync();

            return topCustomers;
        }

        public async Task<IEnumerable<CancellationImpactDto>> GetMostCanceledMovieAndLostRevenueRepoAsync()
        {
            var query = _context.Movies.AsNoTracking();

            var result = await query.Select(m => new CancellationImpactDto()
            {
                MovieId = m.Id,
                MovieTitle = m.Title,
                TotaledCanceledTicket = m.Showtimes
                    .SelectMany(s => s.Reservations)
                    .SelectMany(r => r.ReservationSeats)
                    .Where(rs => rs.Status == Core.Enums.Enums.ReservationStatus.Cancelled)                    
                    .Count(),
                LostRevenue = m.Showtimes
                    .SelectMany(m => m.Reservations)
                    .SelectMany(r => r.ReservationSeats)
                    .Where(r => r.Status == Core.Enums.Enums.ReservationStatus.Cancelled)                    
                    .Sum(rs => rs.Price)

            }).OrderByDescending(s => s.LostRevenue).ToListAsync();

            return result;
        }
    }
}
