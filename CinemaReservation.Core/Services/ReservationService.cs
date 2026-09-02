using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Exceptions;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Core.Enums;

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

            if (showtime.StartTime <= DateTime.UtcNow)
                throw new InvalidOperationException("Cannot book seats for a showtime that has already started.");

            var distinctSeatIds = seatIds.Distinct().ToList();
            var existingSeats = await _reservartinoRepository.GetSeatsByIdsAsync(distinctSeatIds);

            if (existingSeats.Count != distinctSeatIds.Count)
                throw new KeyNotFoundException("One or more selected seats were not found.");

            var bookedSeateIds = await _reservartinoRepository.GetBookedSeatIdsForShowtimeAsync(showtimeId);
            
            bool seatsAlreadyBooked = distinctSeatIds.Any(id => bookedSeateIds.Contains(id));

            if (seatsAlreadyBooked)            
                throw new ConflictException("one or more selected seates are already booked.");

            var priceForTicket = showtime.TicketPrice;
            var reservation = new Reservation()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ShowtimeId= showtimeId,
                TotalPrice = distinctSeatIds.Count * priceForTicket,
                Status = Enums.Enums.ReservationStatus.Confirmed
            };

            var reservationSeats = new List<ReservationSeat>();
            foreach (var seatId in distinctSeatIds)
            {
                reservationSeats.Add(new ReservationSeat()
                {
                    ID = Guid.NewGuid(),
                    ReservationId = reservation.Id,
                    SeatId= seatId,
                    ShowtimeId  = showtimeId,
                    Status = Enums.Enums.ReservationStatus.Confirmed,
                    Price = priceForTicket,
                });
            }        
            
            return await _reservartinoRepository.CommitReservationTransactionAsync(reservation, reservationSeats);            
        }

        public async Task<List<SeatAvailabilityDto>> GetSeatAvailabilityAsync(Guid showtimeId)
        {
            var showtime = await _reservartinoRepository.GetShowtimeByIdAsync(showtimeId);
            if (showtime == null)
                throw new KeyNotFoundException("Showtime not found.");

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
            var result = reservations.Select(r => new UserReservartionDto()
            {
                ReservationId = r.Id,
                ShowtimeId = r.ShowtimeId,
                MovieTitle = r.Showtime?.Movie?.Title ?? "Unknown Movie",
                ShowtimeStart = r.Showtime?.StartTime ?? DateTime.MinValue,
                TotalPrice = r.TotalPrice,
                UserName =  r.User?.Username ?? string.Empty,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt,
                
                Seats = r.ReservationSeats
                .Where(rs => rs.Status == Enums.Enums.ReservationStatus.Confirmed)
                .Select(rs => new ReservedSeatDto()
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
                throw new KeyNotFoundException("Reservation not found.");

            if (reservation.UserId != userId)            
                throw new ForbiddenException("You do not have permission to cancel this reservation.");

            if (reservation.Status == Enums.Enums.ReservationStatus.Cancelled)
                throw new InvalidOperationException("Reservation is already cancelled.");

            EnsureOutsideCancellationWindow(reservation.Showtime);

            await _reservartinoRepository.DeleteReservationAsync(reservation);
        }

        public async Task CancelSingleSeatAsync(Guid userId, Guid reservationId, Guid SeatId)
        {
            var reservation = await _reservartinoRepository.GetReservationWithSeatsByIdAsync(reservationId);

            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found.");

            if (reservation.UserId != userId)
                throw new ForbiddenException("You do not have permission to modify this reservation.");

            if (reservation.Status == Enums.Enums.ReservationStatus.Cancelled)
                throw new InvalidOperationException("Reservation is already cancelled.");

            EnsureOutsideCancellationWindow(reservation.Showtime);

            var confirmedSeats = reservation.ReservationSeats
                .Where(rs => rs.Status == Enums.Enums.ReservationStatus.Confirmed)
                .ToList();

            var seatToRemove = confirmedSeats.FirstOrDefault(rs => rs.SeatId == SeatId);

            if (seatToRemove == null)
                throw new KeyNotFoundException("This seat is not part of your reservation.");

            if (confirmedSeats.Count == 1)
            {
                await _reservartinoRepository.DeleteReservationAsync(reservation);
                return;
            }

            reservation.TotalPrice -= seatToRemove.Price;

            await _reservartinoRepository.UpdatReservationAndRemoveSeatAsync(reservation, seatToRemove);                            
        }

        private static void EnsureOutsideCancellationWindow(Showtime? showtime)
        {
            if (showtime == null)
                throw new InvalidOperationException("Reservation cannot be cancelled because its showtime is missing.");

            if (showtime.StartTime <= DateTime.UtcNow.AddHours(2))
                throw new InvalidOperationException("Reservations cannot be cancelled within 2 hours of the showtime.");
        }
    }
}
