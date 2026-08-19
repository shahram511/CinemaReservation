using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public interface IReservartinoRepository
    {
        Task<List<Seat>> GetAllSestsAsync();
        Task<List<Guid>> GetBookedSeatIdsForShowtimeAsync(Guid showtimeId);
        Task<Showtime?> GetShowtimeByIdAsync(Guid showtimeId);
        Task<List<Seat>> GetSeatsByIdsAsync(List<Guid> seatIds);

        // This method will handle the transaction and concurrency lock
        Task<Reservation> CommitReservationTransactionAsync(Reservation reservation, List<ReservationSeat> reservationSeats);
        Task<List<Reservation>> GetReservationByUserIdAsync(Guid userId); //Define a contract to fetch a users reservations
        Task<Reservation?> GetReservationByRservationIdAsync(Guid reservationId);
        Task DeleteReservationAsync(Reservation reservation);

        // Fetch the reservation including the seats junction table
        Task<Reservation?> GetReservationWithSeatsByIdAsync(Guid reservationId);

        // Remove a singel seat and update the parent reservation in one transaction.
        Task UpdatReservationAndRemoveSeatAsync(Reservation reservation, ReservationSeat seatToRemove);
    }
}
