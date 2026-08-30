using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public interface IReservartionService
    {
        Task<Reservation> CreateReservationAsync(Guid userId, Guid showtimeId, List<Guid> seatIds);
        Task<List<SeatAvailabilityDto>> GetSeatAvailabilityAsync(Guid showtimeId);
        Task<List<UserReservartionDto>> GetUserReservationAsync(Guid userId);
        Task CancelReservationAsync(Guid userId,Guid reservationId);
        Task CancelSingleSeatAsync(Guid userId, Guid reservationId, Guid SeatId);
    }
}
