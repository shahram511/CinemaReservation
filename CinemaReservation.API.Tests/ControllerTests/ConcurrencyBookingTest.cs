using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Entities;
using CinemaReservation.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CinemaReservation.API.Tests.ControllerTests
{
    public class ConcurrencyBookingTest : IClassFixture<PostgresWebApplicationFactory>
    {
        private readonly PostgresWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ConcurrencyBookingTest(PostgresWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateReservation_ShouldPreventDoubleBooking_WhenTwoUsersSelectedSameSeat()
        {
            var testUserId = TestAuthHandler.TestUserId;
            var movieId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();
            var seatId = Guid.NewGuid();

            // seed prerequisites 
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.Users.Add(new User { Id = testUserId, Username = "test" });
                dbContext.Movies.Add(new Movie { Id = movieId, Title = "title", Description = "test", PosterUrl = "test", Genre = "test", DurationInMinutes = 10 });
                dbContext.Showtimes.Add(new Showtime { Id = showtimeId, MovieId = movieId, StartTime = DateTime.UtcNow.AddDays(1) });
                dbContext.Seats.Add(new Seat { Id = seatId, SeatRow = "A", SeatNumber = 2 });

                await dbContext.SaveChangesAsync();
            }

            var requestDto = new CreateReservationDto()
            {
                ShowtimeId = showtimeId,
                SeatIds = new List<Guid> { seatId },
            };
            var jsonString = JsonSerializer.Serialize(requestDto);

            var contentUser1 = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var contentUser2 = new StringContent(jsonString, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");

            // Act
            // we dont use "await" here because we want to start both network requests at the exact same time

            var task1 = _client.PostAsync("/api/reservation", contentUser1);
            var task2 = _client.PostAsync("/api/reservation", contentUser2);

            var results = await Task.WhenAll(task1, task2);// we wait for both parallel requests to finish simultaneously

            // Assert
            var response1 = results[0];
            var response2 = results[1];

            var successCount = results.Count(r => r.IsSuccessStatusCode);
            var conflictCount = results.Count(r =>
                r.StatusCode == HttpStatusCode.BadRequest || 
                r.StatusCode == HttpStatusCode.Conflict|| 
                r.StatusCode ==HttpStatusCode.InternalServerError
                );

            successCount.Should().Be(1);
            conflictCount.Should().Be(1);
        }
    }
}
