using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Entities;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CenemaReservation.E2ETests
{
    public class UserJourneyE2ETests
    {
        // The live url where our API container exposed
        private const string BaseApiUrl = "http://localhost:8080";
        private readonly HttpClient _client;
        public UserJourneyE2ETests()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri(BaseApiUrl)
            };
        }

        [Fact]
        public async Task CompleteUserJourney_LoginAdminMakeMovieAndShowtime_RegisterLoginRegularUser_UserBooking_UserComment_succeeds()
        {
            // 1. =================ADMIN SETUP : LOGIN==================
            var adminLoginDto = new { Username = "shahram", Password = "123456" };
            var adminLoginJsonString =  JsonSerializer.Serialize(adminLoginDto);
            var adminLoginContent = new StringContent(adminLoginJsonString, Encoding.UTF8, "application/json");

            var adminLoginResponse = await _client.PostAsync("/api/auth/login",adminLoginContent);
            adminLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var adminToken = JsonDocument.Parse(await adminLoginResponse.Content.ReadAsStringAsync())
                .RootElement.GetProperty("token").GetString();

            // attach admin JWT for catolog creation
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // 2. ========ADMIN SETUP : CREATE MOVIE AND SHOWTIME========
            var movieDto = new CreateMovieDto()
            {
                Title = "e2e test movie",
                Description = "Test",
                PosterUrl = "test",
                Genre = "test",
                DurationInMinutes = 100
            };
            var movieJsonString = JsonSerializer.Serialize(movieDto);
            var movieContent = new StringContent(movieJsonString, Encoding.UTF8, "application/json");

            var movieResponse = await _client.PostAsync("/api/movies", movieContent);
            movieResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var movieId = JsonDocument.Parse(await movieResponse.Content.ReadAsStringAsync())
                .RootElement.GetProperty("movieId").GetGuid();

            var showtimeDto = new CreateShowtimeDto()
            {
                StartTime = DateTime.UtcNow.AddDays(2),
                
            };
            var showtimeJsonString = JsonSerializer.Serialize(showtimeDto);
            var showtimeContent = new StringContent(showtimeJsonString, Encoding.UTF8, "application/json");

            var showtimeResponse = await _client.PostAsync($"/api/showtime/{movieId}/showtimes", showtimeContent);
            showtimeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var showtimeId = JsonDocument.Parse(await showtimeResponse.Content.ReadAsStringAsync())
                .RootElement.GetProperty("showtimeId").GetGuid();

            // 2. =================REGISTER A NEW USER (REGULAR USER)==================
            // generate a random email
            var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            var userPassword = "securepassword";

            // prepare the registration payload using our standard DTO
            var registerDto = new RegisterUserDto()
            {
                Email = $"e2e_{uniqueSuffix}@.com",
                Username = $"user_{uniqueSuffix}",
                Password = userPassword,                
            };

            var jsonString = JsonSerializer.Serialize(registerDto);
            var registerContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // send a real network request to the rinning container
            var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);          

            // assert that the user was successfully persisted in the real PostgreSQL database
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // ============LOG IN AND EXTRACT JWT (REGULAR USER)================
            var loginDto = new LoginUserDto()
            {
                Username = registerDto.Username,
                Password = userPassword,
            };

            var loginjsonString = JsonSerializer.Serialize(loginDto);
            var loginContent = new StringContent(loginjsonString, Encoding.UTF8, "application/json");

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // read and pars the JSON response body to extract the real JWT
            var loginResponseBody = await loginResponse.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(loginResponseBody);

            // match the exact casing our API returns token
            var tokenFound = jsonDoc.RootElement.TryGetProperty("token", out var token);
            tokenFound.Should().BeTrue();

            var jwtToken = token.GetString();
            jwtToken.Should().NotBeNullOrWhiteSpace();

            // attach the real JWT token to all subsequent requests for this client
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            // ======FETCH CATALOG ABAILABLE SEATES======
            // call the endpoin to get available seats for this specific showtime
            var seatsResponse = await _client.GetAsync($"/api/showtime/{showtimeId}/seats");
            seatsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var seatsJson = await seatsResponse.Content.ReadAsStringAsync();
            using var seatsDoc = JsonDocument.Parse(seatsJson);

            // extract the ID of the first available seat in the array
            var availableSeatId = seatsDoc.RootElement
                .EnumerateArray()
                .First(seat => seat.GetProperty("isAvailable").GetBoolean() == true)
                .GetProperty("id")
                .GetGuid();
            

            // BOOK THE RESERVATION
            // build the payload mapping to our reservation DTO
            var reservationDto = new CreateReservationDto()
            {
                ShowtimeId = showtimeId,
                SeatIds = new List<Guid> {availableSeatId}
            };

            var reservationJson = JsonSerializer.Serialize(reservationDto);
            var reservationContent = new StringContent(reservationJson, Encoding.UTF8, "application/json");

            // send the authenticated POST request to lock the seat
            var bookingResponse = await _client.PostAsync("/api/reservation", reservationContent);
            bookingResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            //=====LEAVE A COMMENT =====
            //build the payload for the mongoDb comment
            var commentDto = new CreateCommentDto()
            {
                Text = "this is the e2e test",
                Rating = 5
            };

            var commentJson = JsonSerializer.Serialize(commentDto);
            var commentContent = new StringContent(commentJson, Encoding.UTF8, "application/json");

            var commentResponse = await _client.PostAsync($"/api/movie/{movieId}/MovieComment", commentContent);

            commentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
}
