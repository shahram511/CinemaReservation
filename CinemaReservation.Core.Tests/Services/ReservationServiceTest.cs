using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Enums;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Core.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Tests.Services
{
    public class ReservationServiceTest
    {
        private readonly Mock<IReservartinoRepository> _repositoryMock;
        private readonly ReservationService _service;

        public ReservationServiceTest()
        {
            // Build Mock Database
            _repositoryMock = new Mock<IReservartinoRepository>();
            _service = new ReservationService(_repositoryMock.Object);
        }

        [Fact]
        public async Task CreateReservation_ShouldThrowException_WhenSeatIsAlreadyBooked()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();

            var requestedSeatIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            _repositoryMock
                .Setup(repo => repo.GetShowtimeByIdAsync(showtimeId))
                .ReturnsAsync(new Showtime());

            var bookedSeatInDb = new List<Guid> { requestedSeatIds[0] };

            _repositoryMock
                .Setup(repo => repo.GetBookedSeatIdsForShowtimeAsync(showtimeId))
                .ReturnsAsync(bookedSeatInDb);

            // Act
            Func<Task> action = async () =>
                await _service.CreateReservationAsync(userId, showtimeId, requestedSeatIds);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("one or more selected seates are already booked.");
        }

        [Fact]
        public async Task CreateReservatoin_ShouldReturnReseravtion_WhenEverythingIsCorrect()
        {
            // Arrange-------
            var userId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();

            var requesedSeatIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }; // we are requesting exactly 2 seats 

            // scenario setup 1: The showtime exists in the database
            _repositoryMock
                .Setup(repo => repo.GetShowtimeByIdAsync(showtimeId))
                .ReturnsAsync(new Showtime());

            // scenario setup 2: No seats are currently booked for this showtime
            _repositoryMock
                .Setup(repo => repo.GetBookedSeatIdsForShowtimeAsync(showtimeId))
                .ReturnsAsync(new List<Guid>());

            // scenario setup 3: Simulate a successful database commit
            var expectedReservation = new Reservation { Id = Guid.NewGuid() };
            _repositoryMock
                .Setup(repo => repo.CommitReservationTransactionAsync(It.IsAny<Reservation>(), It.IsAny<List<ReservationSeat>>()))
                .ReturnsAsync(expectedReservation);

            // Act---------
            var result = await _service.CreateReservationAsync(userId, showtimeId, requesedSeatIds);

            // Assert------
            // verify 1: The method should return the exact reservation object we mocked
            result.Should().NotBeNull();
            result.Should().Be(expectedReservation);

            // verify 2: We verify if CommitReservationTransactionAsync was called EXACTLY ONCE with the correctly
            // calculated data, 2 seats * 15.00m = 30.00m
            _repositoryMock
                .Verify(repo => repo.CommitReservationTransactionAsync(
                    It.Is<Reservation>(r => r.TotalPrice == 30.00m && r.UserId == userId),
                    It.Is<List<ReservationSeat>>(seats => seats.Count == 2)
                    ), Times.Once);
        }

        [Fact]
        public async Task CreateReservation_ShouldThrowException_WhenShowtimeIsNotFound()
        {
            // Arrange----
            var userId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();
            var requestedSeatId = new List<Guid> { Guid.NewGuid() };

            _repositoryMock
                .Setup(repo => repo.GetShowtimeByIdAsync(showtimeId))
                .ReturnsAsync((Showtime?)null);

            // Act-----
            Func<Task> action = async () =>
            await _service.CreateReservationAsync(userId, showtimeId, requestedSeatId);

            // Assert----
            // verify 1: check if the exact exception and message are thrown
            await action.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Showtime not found.");

            // verify 2: since the throws an error at the if(showtime= null)... it should NEVER reach the code
            // that fetches booked seats. we use Times.Never to prove this!
            _repositoryMock.Verify(repo => repo.GetBookedSeatIdsForShowtimeAsync(It.IsAny<Guid>()),Times.Never);
        }
    }
}
