using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Enums;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
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
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
            {                
                throw new InvalidOperationException("Seat is already reserved.");
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
                    rs.Reservation.Status != Enums.ReservationStatus.Cancelled && rs.Status== Enums.ReservationStatus.Confirmed)
                    
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
               .Where(rs => rs.Status == Enums.ReservationStatus.Confirmed)
               .ToListAsync();
        }

        // Delete one reservation------------
        public async Task<Reservation?> GetReservationByRservationIdAsync(Guid reservationId)
        {
            return await _context.Reservations
                .Include(r => r.ReservationSeats)
                .Include(rs => rs.Showtime)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        // Delete one special seate----------
        public async Task<Reservation?>  GetReservationWithSeatsByIdAsync(Guid reservationId)
        {
            return await _context.Reservations
                .Include(r => r.Showtime)
                .Include(r => r.ReservationSeats)                
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        public async Task DeleteReservationAsync(Reservation reservation)
        {
            //  Cancel the parent reservation
            reservation.Status = Enums.ReservationStatus.Cancelled;

            if (reservation.ReservationSeats != null)
            {
                foreach (var seat in reservation.ReservationSeats)
                {
                    seat.Status = Enums.ReservationStatus.Cancelled;
                }
            }

            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatReservationAndRemoveSeatAsync(Reservation reservation, ReservationSeat seatToRemove)
        {
            // SOFT DELETE : insted of using the Remove() we  just change the status of the seat
            seatToRemove.Status = Enums.ReservationStatus.Cancelled;

            //Update the parent reservation (because we will modify its TotalPrice
            _context.Reservations.Update(reservation);

            await _context.SaveChangesAsync();
        }
    }
}
