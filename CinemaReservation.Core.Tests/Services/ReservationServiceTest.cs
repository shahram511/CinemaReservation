using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Enums;
using CinemaReservation.Core.Exceptions;
using ReservationStatus = CinemaReservation.Core.Enums.Enums.ReservationStatus;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Core.Services;
using FluentAssertions;
using Moq;

namespace CinemaReservation.Core.Tests.Services
{
    public class ReservationServiceTest
    {
        private readonly Mock<IReservartinoRepository> _repositoryMock;
        private readonly ReservationService _service;

        public ReservationServiceTest()
        {
            _repositoryMock = new Mock<IReservartinoRepository>();
            _service = new ReservationService(_repositoryMock.Object);
        }

        private static Showtime FutureShowtime(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1),
            TicketPrice = 15.00m
        };

        [Fact]
        public async Task CreateReservation_ShouldThrowException_WhenSeatIsAlreadyBooked()
        {
            var userId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();
            var requestedSeatIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            _repositoryMock
                .Setup(repo => repo.GetShowtimeByIdAsync(showtimeId))
                .ReturnsAsync(FutureShowtime(showtimeId));

            _repositoryMock
                .Setup(repo => repo.GetSeatsByIdsAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(requestedSeatIds.Select(id => new Seat { Id = id }).ToList());

            _repositoryMock
                .Setup(repo => repo.GetBookedSeatIdsForShowtimeAsync(showtimeId))
                .ReturnsAsync(new List<Guid> { requestedSeatIds[0] });

            Func<Task> action = async () =>
                await _service.CreateReservationAsync(userId, showtimeId, requestedSeatIds);

            await action.Should().ThrowAsync<ConflictException>()
                .WithMessage("one or more selected seates are already booked.");
        }

        [Fact]
        public async Task CreateReservatoin_ShouldReturnReseravtion_WhenEverythingIsCorrect()
        {
            var userId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();
            var requesedSeatIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            _repositoryMock
                .Setup(repo => repo.GetShowtimeByIdAsync(showtimeId))
                .ReturnsAsync(FutureShowtime(showtimeId));

            _repositoryMock
                .Setup(repo => repo.GetSeatsByIdsAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(requesedSeatIds.Select(id => new Seat { Id = id }).ToList());

            _repositoryMock
                .Setup(repo => repo.GetBookedSeatIdsForShowtimeAsync(showtimeId))
                .ReturnsAsync(new List<Guid>());

            var expectedReservation = new Reservation { Id = Guid.NewGuid() };
            _repositoryMock
                .Setup(repo => repo.CommitReservationTransactionAsync(It.IsAny<Reservation>(), It.IsAny<List<ReservationSeat>>()))
                .ReturnsAsync(expectedReservation);

            var result = await _service.CreateReservationAsync(userId, showtimeId, requesedSeatIds);

            result.Should().NotBeNull();
            result.Should().Be(expectedReservation);

            _repositoryMock
                .Verify(repo => repo.CommitReservationTransactionAsync(
                    It.Is<Reservation>(r => r.TotalPrice == 30.00m && r.UserId == userId),
                    It.Is<List<ReservationSeat>>(seats => seats.Count == 2)
                    ), Times.Once);
        }

        [Fact]
        public async Task CreateReservation_ShouldThrowException_WhenShowtimeIsNotFound()
        {
            var userId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();
            var requestedSeatId = new List<Guid> { Guid.NewGuid() };

            _repositoryMock
                .Setup(repo => repo.GetShowtimeByIdAsync(showtimeId))
                .ReturnsAsync((Showtime?)null);

            Func<Task> action = async () =>
                await _service.CreateReservationAsync(userId, showtimeId, requestedSeatId);

            await action.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Showtime not found.");

            _repositoryMock.Verify(repo => repo.GetBookedSeatIdsForShowtimeAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateReservation_ShouldThrow_WhenShowtimeHasStarted()
        {
            var showtimeId = Guid.NewGuid();
            var seatIds = new List<Guid> { Guid.NewGuid() };

            _repositoryMock
                .Setup(repo => repo.GetShowtimeByIdAsync(showtimeId))
                .ReturnsAsync(new Showtime { Id = showtimeId, StartTime = DateTime.UtcNow.AddMinutes(-1) });

            Func<Task> action = async () =>
                await _service.CreateReservationAsync(Guid.NewGuid(), showtimeId, seatIds);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot book seats for a showtime that has already started.");
        }

        [Fact]
        public async Task CreateReservation_ShouldThrow_WhenSeatDoesNotExist()
        {
            var showtimeId = Guid.NewGuid();
            var seatIds = new List<Guid> { Guid.NewGuid() };

            _repositoryMock
                .Setup(repo => repo.GetShowtimeByIdAsync(showtimeId))
                .ReturnsAsync(FutureShowtime(showtimeId));

            _repositoryMock
                .Setup(repo => repo.GetSeatsByIdsAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<Seat>());

            Func<Task> action = async () =>
                await _service.CreateReservationAsync(Guid.NewGuid(), showtimeId, seatIds);

            await action.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("One or more selected seats were not found.");
        }

        [Fact]
        public async Task GetSeatAvailability_ShouldThrow_WhenShowtimeIsNotFound()
        {
            var showtimeId = Guid.NewGuid();
            _repositoryMock
                .Setup(repo => repo.GetShowtimeByIdAsync(showtimeId))
                .ReturnsAsync((Showtime?)null);

            Func<Task> action = async () => await _service.GetSeatAvailabilityAsync(showtimeId);

            await action.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Showtime not found.");
        }

        [Fact]
        public async Task CancelSingleSeat_ShouldCancelWholeReservation_WhenLastConfirmedSeatIsRemoved()
        {
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();
            var seatId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                Status = ReservationStatus.Confirmed,
                Showtime = FutureShowtime(),
                ReservationSeats =
                {
                    new ReservationSeat
                    {
                        SeatId = seatId,
                        Status = ReservationStatus.Cancelled
                    },
                    new ReservationSeat
                    {
                        SeatId = seatId,
                        Status = ReservationStatus.Confirmed,
                        Price = 15.00m
                    }
                }
            };

            _repositoryMock
                .Setup(repo => repo.GetReservationWithSeatsByIdAsync(reservationId))
                .ReturnsAsync(reservation);

            await _service.CancelSingleSeatAsync(userId, reservationId, seatId);

            _repositoryMock.Verify(repo => repo.DeleteReservationAsync(reservation), Times.Once);
            _repositoryMock.Verify(repo => repo.UpdatReservationAndRemoveSeatAsync(It.IsAny<Reservation>(), It.IsAny<ReservationSeat>()), Times.Never);
        }

        [Fact]
        public async Task CancelSingleSeat_ShouldKeepReservation_WhenOtherConfirmedSeatsRemain()
        {
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();
            var seatToRemove = Guid.NewGuid();
            var seatToKeep = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                TotalPrice = 30.00m,
                Status = ReservationStatus.Confirmed,
                Showtime = FutureShowtime(),
                ReservationSeats =
                {
                    new ReservationSeat { SeatId = seatToRemove, Status = ReservationStatus.Confirmed, Price = 15.00m },
                    new ReservationSeat { SeatId = seatToKeep, Status = ReservationStatus.Confirmed, Price = 15.00m }
                }
            };

            _repositoryMock
                .Setup(repo => repo.GetReservationWithSeatsByIdAsync(reservationId))
                .ReturnsAsync(reservation);

            await _service.CancelSingleSeatAsync(userId, reservationId, seatToRemove);

            reservation.TotalPrice.Should().Be(15.00m);
            _repositoryMock.Verify(repo => repo.DeleteReservationAsync(It.IsAny<Reservation>()), Times.Never);
            _repositoryMock.Verify(repo => repo.UpdatReservationAndRemoveSeatAsync(reservation, It.Is<ReservationSeat>(s => s.SeatId == seatToRemove)), Times.Once);
        }
    }
}
