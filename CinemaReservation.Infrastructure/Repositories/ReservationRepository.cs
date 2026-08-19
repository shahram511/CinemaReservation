using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Enums;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CinemaReservation.Infrastructure.Repositories
{
    public class ReservationRepository : IReservartinoRepository
    {
        private readonly ApplicationDbContext _context;

        public ReservationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Seat>> GetSeatsByIdsAsync(List<Guid> seatIds)
        {
            return await _context.Seats.Where(s => seatIds.Contains(s.Id)).ToListAsync();
        }

        public async Task<Showtime?> GetShowtimeByIdAsync(Guid showtimeId)
        {
            return await _context.Showtimes.FindAsync(showtimeId);
        }

        public async Task<Reservation> CommitReservationTransactionAsync(Reservation reservation, List<ReservationSeat> reservationSeats)
        {
            // make a new workspace  with tis line
            //using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                _context.Reservations.Add(reservation);
                _context.ReservationSeats.AddRange(reservationSeats);

                await _context.SaveChangesAsync();               
                return reservation; 
            }
            catch (Exception ex)
            {
                //throw new InvalidOperationException("One or more of the selected seats were just booked by someone else");
                string realErroMessage = ex.InnerException != null ? ex.InnerException.Message:ex.Message;
                throw new Exception(realErroMessage);
            }
        }

        public async Task<List<Seat>> GetAllSestsAsync()
        {
            return await _context.Seats
                .OrderBy(s => s.SeatRow)
                .ThenBy(s => s.SeatNumber)
                .ToListAsync();
                
        }

        public async Task<List<Guid>> GetBookedSeatIdsForShowtimeAsync(Guid showtimeId)
        {
            return await _context.ReservationSeats
                .Where(rs => rs.Reservation.ShowtimeId == showtimeId &&
                    rs.Reservation.Status != Enums.ReservationStatus.Cancelled)
                .Select(rs => rs.SeatId)
                .ToListAsync();
        }

        public async Task<List<Reservation>> GetReservationByUserIdAsync(Guid userId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(r => r.ReservationSeats)
                    .ThenInclude(rs => rs.Seat)
               .Where(r => r.UserId == userId)
               .ToListAsync();
        }

        // Delete one reservation------------
        public async Task<Reservation?> GetReservationByRservationIdAsync(Guid reservationId)
        {
            return await _context.Reservations
                .Include(r => r.Showtime)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        public async Task DeleteReservationAsync(Reservation reservation)
        {
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
        }

        // Delete one special seate----------
        public async Task<Reservation?> GetReservationWithSeatsByIdAsync(Guid reservationId)
        {
            return await _context.Reservations
                .Include(r => r.Showtime)
                .Include(r => r.ReservationSeats)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        public async Task UpdatReservationAndRemoveSeatAsync(Reservation reservation, ReservationSeat seatToRemove)
        {
            _context.ReservationSeats.Remove(seatToRemove);

            //Update the parent reservation (because we will modify its TotalPrice
            _context.Reservations.Update(reservation);

            await _context.SaveChangesAsync();
        }
    }
}
