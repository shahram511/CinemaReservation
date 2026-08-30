using CinemaReservation.Core.Entities;
using CinemaReservation.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using CinemaReservation.Core.DTOs;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CinemaReservation.Core.Enums;


namespace CinemaReservation.API.Tests.ControllerTests
{
    public class ReservationControllerTest  :IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ReservationControllerTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetMyReservation_ShouldReturnUserReservation_WhenAuthorized()
        {
            // Arrange            
            var testUserId = TestAuthHandler.TestUserId;
            var showtimeId = Guid.NewGuid();
            var movieId = Guid.NewGuid();
            var seatId = Guid.NewGuid();            

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.Users.Add(new User{Id = testUserId,Username = "test"});
                dbContext.Movies.Add(new Movie { Id = movieId, Title = "title", Description = "test", PosterUrl = "test", Genre = "test", DurationInMinutes = 10 });
                dbContext.Showtimes.Add(new Showtime{Id = showtimeId, MovieId = movieId, StartTime = DateTime.Now.AddDays(1)});
                dbContext.Seats.Add(new Seat{ Id = seatId, SeatRow = "A", SeatNumber = 2});

                var userReservation = new Reservation()
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,   
                    ShowtimeId = showtimeId,
                    TotalPrice = 10.00m,
                    Status = Core.Enums.Enums.ReservationStatus.Confirmed                    
                };                

                dbContext.Reservations.Add(userReservation);

                dbContext.ReservationSeats.Add(new Core.Enums.ReservationSeat { ReservationId = userReservation.Id, SeatId = seatId, Status = Core.Enums.Enums.ReservationStatus.Confirmed });

                await dbContext.SaveChangesAsync();
            }

            // attach the fake authentication scheme
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");

            // Act

            // send the GET request to the endpoint
            var response = await _client.GetAsync("/api/reservation/my-reservations");

            // Assert
            // verify the request passed the authorization check and succeeded
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // read and verify the JSON data
            var jsonString = await response.Content.ReadAsStringAsync();

            var returnedReservations = JsonSerializer.Deserialize<List<UserReservartionDto>>(jsonString, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            // prove that API only returned the reservation belonging to our user
            returnedReservations.Should().NotBeNull();            
            returnedReservations[0].MovieTitle.Should().Be("title");
            returnedReservations[0].Seats.Should().ContainSingle();
            returnedReservations.Should().ContainSingle();
        }

        [Fact]
        public async Task CreateReservation_ShouldReturnSuccess_AndSaveToDatabase()
        {
            var testUserId = TestAuthHandler.TestUserId;
            var showtimeId = Guid.NewGuid();
            var movieId = Guid.NewGuid();
            var seatId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                // Arrange
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                dbContext.Users.Add(new User { Id = testUserId, Username = "test" });
                dbContext.Movies.Add(new Movie { Id = movieId, Title = "title", Description = "test", PosterUrl = "test", Genre = "test", DurationInMinutes = 10 });
                dbContext.Showtimes.Add(new Showtime { Id = showtimeId, MovieId = movieId, StartTime = DateTime.Now.AddDays(1) });
                dbContext.Seats.Add(new Seat { Id = seatId, SeatRow = "A", SeatNumber = 2 });

                await dbContext.SaveChangesAsync();            
            }

            var requestDto = new CreateReservationDto()
            {
                ShowtimeId = showtimeId,
                SeatIds = new List<Guid> { seatId },
            };

            var jsonString = JsonSerializer.Serialize(requestDto);
            var httpContent = new  StringContent(jsonString, Encoding.UTF8, "application/json"); 

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");

            // Act
            var reseponse = await _client.PostAsync("/api/reservation",httpContent );

            // Assert
            reseponse.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // find all reservation blonging to our fake user
                var userReservation = await dbContext.Reservations
                    .Include(r => r.ReservationSeats)
                    .Where(r => r.UserId == testUserId)
                    .ToListAsync();

                userReservation.Should().ContainSingle();

                // prove the reservation link to the correct Showtime
                var savedReservation = userReservation.First();
                savedReservation.ShowtimeId.Should().Be(showtimeId);

                // prove the bridg table (ReservationSeates) successfully linked the correct Seat
                savedReservation.ReservationSeats.Should().ContainSingle();
                savedReservation.ReservationSeats.First().SeatId.Should().Be(seatId);
            }                       
        }

        [Fact]
        public async Task CencelReservation_ShouldRemoveCompleteReservation_WhenAuthorized()
        {
            var testUserId = TestAuthHandler.TestUserId;
            var reservationId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();
            var movieId = Guid.NewGuid();
            var seat1Id = Guid.NewGuid();
            var seat2Id = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.Users.Add(new User { Id = testUserId, Username = "test" });
                dbContext.Movies.Add(new Movie { Id = movieId, Title = "title", Description = "test", PosterUrl = "test", Genre = "test", DurationInMinutes = 10 });
                dbContext.Showtimes.Add(new Showtime { Id = showtimeId, MovieId = movieId, StartTime = DateTime.Now.AddDays(1) });
                dbContext.Seats.Add(new Seat { Id = seat1Id, SeatRow = "A", SeatNumber = 1 });
                dbContext.Seats.Add(new Seat { Id = seat2Id, SeatRow = "A", SeatNumber = 2 });

                var reservation = new Reservation()
                {
                    Id = reservationId,
                    UserId = testUserId,
                    ShowtimeId = showtimeId,
                    Status = Enums.ReservationStatus.Confirmed,
                    TotalPrice = 30.00m
                };

                dbContext.Reservations.Add(reservation);

                dbContext.ReservationSeats.AddRange(
                    new ReservationSeat
                    {
                        ID = Guid.NewGuid(),
                        ReservationId = reservationId,
                        SeatId = seat1Id,
                        ShowtimeId = showtimeId,
                        Status = Enums.ReservationStatus.Confirmed,
                        Price = 15.00m
                    },
                    new ReservationSeat
                    {
                        ID = Guid.NewGuid(),
                        ReservationId = reservationId,
                        SeatId = seat2Id,
                        ShowtimeId = showtimeId,
                        Status = Enums.ReservationStatus.Confirmed,
                        Price = 15.00m
                    });

                await dbContext.SaveChangesAsync();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Testschme");

            // Act
            var response = await _client.DeleteAsync($"/api/reservation/{reservationId}");

            // Assert 
            response.IsSuccessStatusCode.Should().BeTrue();
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var deletReservation = await dbContext.Reservations
                    .Include(r => r.ReservationSeats)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);

                deletReservation.Should().NotBeNull();
                deletReservation.Status.Should().Be(Enums.ReservationStatus.Cancelled);
                deletReservation.ReservationSeats.Should().OnlyContain(rs => rs.Status == Enums.ReservationStatus.Cancelled);
            }            
        }

        [Fact]
        public async Task CancelSingleReservation_ShouldRemoveSpecificSeats_AndKeepReservationActive()
        {
            // Arrange 
            var testUserId = TestAuthHandler.TestUserId;
            var reservationId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();
            var movieId = Guid.NewGuid();
            var seatToRemoveId = Guid.NewGuid();
            var seatToKeepId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.Users.Add(new User { Id = testUserId, Username = "test" });
                dbContext.Movies.Add(new Movie { Id = movieId, Title = "title", Description = "test", PosterUrl = "test", Genre = "test", DurationInMinutes = 10 });
                dbContext.Showtimes.Add(new Showtime { Id = showtimeId, MovieId = movieId, StartTime = DateTime.Now.AddDays(1) });
                dbContext.Seats.Add(new Seat { Id = seatToRemoveId, SeatRow = "A", SeatNumber = 1 });
                dbContext.Seats.Add(new Seat { Id = seatToKeepId, SeatRow = "A", SeatNumber = 2 });

                dbContext.Reservations.Add(
                    new Reservation
                    {
                        Id = reservationId,
                        UserId = testUserId,
                        ShowtimeId = showtimeId,
                        Status = Enums.ReservationStatus.Confirmed,
                        TotalPrice = 30.00m
                    });

                dbContext.ReservationSeats.AddRange(
                    new ReservationSeat { ID = Guid.NewGuid(), ReservationId = reservationId, SeatId = seatToKeepId, ShowtimeId = showtimeId, Status = Enums.ReservationStatus.Confirmed, Price = 15.00m },
                    new ReservationSeat { ID = Guid.NewGuid(), ReservationId = reservationId, SeatId = seatToRemoveId, ShowtimeId = showtimeId, Status = Enums.ReservationStatus.Confirmed, Price = 15.00m }
                    );                                                     

                await dbContext.SaveChangesAsync();
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");

            // Act 
            var response = await _client.DeleteAsync($"/api/reservation/{reservationId}/seats/{seatToRemoveId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var updatReservation = await dbContext.Reservations
                    .Include(r => r.ReservationSeats)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);

                updatReservation.Should().NotBeNull();
                updatReservation.Status.Should().Be(Enums.ReservationStatus.Confirmed); // parent reservation must be active

                updatReservation.ReservationSeats.Should()
                    .Contain(rs => rs.SeatId== seatToRemoveId && rs.Status == Enums.ReservationStatus.Cancelled);

                updatReservation.ReservationSeats.Should()
                    .Contain(rs => rs.SeatId == seatToKeepId && rs.Status == Enums.ReservationStatus.Confirmed);

                updatReservation.TotalPrice.Should().Be(15.00m);
            }
        }
    }
}

