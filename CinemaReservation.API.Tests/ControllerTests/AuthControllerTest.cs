using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;


namespace CinemaReservation.API.Tests.ControllerTests
{
    [Collection("SharedDatabaseCollection")]
    public class AuthControllerTest : IntegrationTestBase
    {
        public AuthControllerTest(SharedDatabaseFixture fixture) : base(fixture)
        {            
        }

        [Fact]
        public async Task Register_ShouldRetrurnOk_WhenRequestIsValid()
        {
            // Arrange
            var requestDto = new RegisterUserDto()
            {
                Username = "Test",
                Email = "test@cinema.com",
                Password = "password"
            };

            var jsonString = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // Act
            var resepons = await Client.PostAsync("/api/auth/register", content);

            // Assert 
            resepons.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseText = await resepons.Content.ReadAsStringAsync();
            responseText.Should().Contain("user registerd successfully");

            // verify database state
            using (var scope = Factory.Services.CreateScope())
            {
                var dbContenxt = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userExists = await dbContenxt.Users.AnyAsync(u => u.Username == "Test");

                userExists.Should().BeTrue();   
            }
        }

        [Fact]
        public async Task Login_ShoulReturnToken_WhenCredentialIsValid()
        {
            // Arrang
            var testUsername = "LoginTest";
            var testPassword = "ValidPassword";

            using (var scope = Factory.Services.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                await authService.RegisterUserAsync(testUsername, "login@cinema.com", testPassword);
            }

            var requestDto = new LoginUserDto()
            {
                Username  = testUsername,
                Password = testPassword
            };

            var jsonString = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            
            // Act 
            var response  = await Client.PostAsync("/api/auth/login",content);

            // Assert  
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();

            // pare the JSON object returning
            using var jsonDocument = JsonDocument.Parse(responseString);
            var root = jsonDocument.RootElement;

            root.TryGetProperty("token", out var tokenElement).Should().BeTrue();
            tokenElement.GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Register_ShoulReturnBadRequest_WhenValidationFails()
        {
            // Arrang
            // create an invalid DTO (e.g. missing password, invalid email format)
            var requestDto = new RegisterUserDto()
            {
                Username = "A",
                Email = "this is not an email",
                Password = "short"
            };

            var jsonString = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/register", content);

            // Assert 
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
        {
            using (var scope = Factory.Services.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                await authService.RegisterUserAsync("WrongPassUser", "wrongpass@cinema.com", "ValidPassword");
            }

            var requestDto = new LoginUserDto
            {
                Username = "WrongPassUser",
                Password = "incorrect"
            };

            var jsonString = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/auth/login", content);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
