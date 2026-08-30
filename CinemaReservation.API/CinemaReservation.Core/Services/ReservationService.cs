using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Enums;
using CinemaReservation.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;




namespace CinemaReservation.Core.Services
{
    public class ReservationService : IReservartionService
    {
        private readonly IReservartinoRepository _reservartinoRepository;

        public ReservationService(IReservartinoRepository reservartinoRepository)
        {
            _reservartinoRepository = reservartinoRepository;
        }



        public async Task<Reservation> CreateReservationAsync(Guid userId, Guid showtimeId, List<Guid> seatIds)
        {
            var showtime = await _reservartinoRepository.GetShowtimeByIdAsync(showtimeId);

            if (showtime == null)            
                throw new KeyNotFoundException("Showtime not found.");
            

            // Fetch the list of seats that are already booked for this showtime
            var bookedSeateIds = await _reservartinoRepository.GetBookedSeatIdsForShowtimeAsync(showtimeId);
            
            // Check if any of the requested seat IDs intersect with the already booked seat IDs
            bool seatsAlreadyBooked = seatIds.Any(id => bookedSeateIds.Contains(id));

            if (seatsAlreadyBooked)            
                throw new KeyNotFoundException("one or more selected seates are already booked.");
            

            var priceForTicket = 15.00m;
            // Business Logic: Calculate price(e.g. 15$ per seat)
            var reservation = new Reservation()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ShowtimeId= showtimeId,
                TotalPrice = seatIds.Count*priceForTicket,
                Status = Enums.Enums.ReservationStatus.Confirmed
            };


            var reservationSeats = new List<ReservationSeat>();
            foreach (var seatId in seatIds)
            {
                reservationSeats.Add(new ReservationSeat()
                {
                    ID = Guid.NewGuid(),
                    ReservationId = reservation.Id,
                    SeatId= seatId,
                    ShowtimeId  = showtimeId,
                    Price = 15.00m,
                    
                    
                });
            }            
                //pass the constructes objects to the infrastructure layer to handle the secure database transaction
                return await _reservartinoRepository.CommitReservationTransactionAsync(reservation, reservationSeats);            
        }

        public async Task<List<SeatAvailabilityDto>> GetSeatAvailabilityAsync(Guid showtimeId)
        {
            // Fetch the raw data from the database using our two separate, lightweight repository calls
            var allSeats = await _reservartinoRepository.GetAllSestsAsync();
            var bookedSeatsIds = await _reservartinoRepository.GetBookedSeatIdsForShowtimeAsync(showtimeId);

            return allSeats.Select(seat => new SeatAvailabilityDto()
            {
                Id = seat.Id,
                Row = seat.SeatRow,
                Number = seat.SeatNumber,
                IsAvailable = !bookedSeatsIds.Contains(seat.Id)
            }).ToList();
        }

        public async Task<List<UserReservartionDto>> GetUserReservationAsync(Guid userId)
        {
            var reservations = await _reservartinoRepository.GetReservationByUserIdAsync(userId);
            // Use LINQ to map the list of reservation entities into a list of UserRservationDto objects.
            var result = reservations.Select(r => new UserReservartionDto()
            {
                ReservationId = r.Id,
                ShowtimeId = r.ShowtimeId,
                MovieTitle = r.Showtime?.Movie?.Title ?? "Unknown Movie",
                ShowtimeStart = r.Showtime?.StartTime ?? DateTime.MinValue,
                TotalPrice = r.TotalPrice,
                UserName =  r.User.Username,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt,
                
                Seats = r.ReservationSeats.Select(rs => new ReservedSeatDto()
                
                {
                    SeatId = rs.SeatId,
                    Row = rs.Seat?.SeatRow ?? "",
                    Number = rs.Seat?.SeatNumber ?? 0
                }).ToList()

            }).ToList(); 

            return result; 

        }
        public async Task CancelReservationAsync(Guid userId, Guid reservationId)
        {
            var reservation = await _reservartinoRepository.GetReservationByRservationIdAsync(reservationId);

            if (reservation == null)            
                throw new Exception("Reservation not found,");
            

            if (reservation.UserId!= userId)            
                throw new Exception("You do not permission to cancel this reservation.");
            

            var currentTime = DateTime.UtcNow;

            if (reservation.Showtime?.StartTime <= currentTime.AddHours(2))            
                throw new Exception("Reservations cannot be cancelled within 1 hour of the showtime.");
            

            await _reservartinoRepository.DeleteReservationAsync(reservation);
        }

        public async Task CancelSingleSeatAsync(Guid userId, Guid reservationId, Guid SeatId)
        {
            var reservation = await _reservartinoRepository.GetReservationWithSeatsByIdAsync(reservationId);

            if (reservation == null)
                throw new Exception("Reservation not found.");

            if (reservation.UserId != userId)
                throw new Exception("You do not have permission to midigy this reservation");

            var currentTime = DateTime.UtcNow;

            if (reservation.Showtime?.StartTime <= currentTime.AddHours(2))
                throw new Exception("Reservation cannot be modified within 2 hours of the showtime.");

            var seatToRemove = reservation.ReservationSeats.FirstOrDefault(rs => rs.SeatId == SeatId);

            if (seatToRemove == null)
                throw new Exception("This seat is not part of your reservation");

            // If this iis the vety last seat in the reservation, just delete the whole reservation
            if (reservation.ReservationSeats.Count == 1)
            {
                await _reservartinoRepository.DeleteReservationAsync(reservation);
                return;
            }

            reservation.TotalPrice -= seatToRemove.Price;

            await _reservartinoRepository.UpdatReservationAndRemoveSeatAsync(reservation, seatToRemove);                            
        }
    }
}
