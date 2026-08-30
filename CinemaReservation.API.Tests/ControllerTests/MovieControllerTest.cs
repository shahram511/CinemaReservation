using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Entities;
using CinemaReservation.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace CinemaReservation.API.Tests.ControllerTests
{
    public class MovieControllerTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public MovieControllerTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetMovie_ShouldReturnSuccessStatusCode()
        {
            // Arrang            
            var url = "/api/movies";  // The endpoin url

            // Act
            var response = await _client.GetAsync(url); // Sending  a real HTTP GET request  to the API

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK); // We expect  a 200 OK status code

            // We can ensure the response is not empty
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetMovieId_ShouldReturnMovie_WhenMovieExistsInDatabase()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var expectedTitle = "inception";

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var movie = new Movie()
                {
                    Id = movieId,
                    Title = expectedTitle,
                    Description = "A Grat Movie",
                    Genre = "action",
                    DurationInMinutes = 120
                };

                dbContext.Movies.Add(movie);
                await dbContext.SaveChangesAsync();
            }
                // Act(send a request)
            var response = await _client.GetAsync($"/api/movies/{movieId}");

            // Assert (verify a response)
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var jsonString = await response.Content.ReadAsStringAsync();

            var returnedMovie = JsonSerializer.Deserialize<Movie>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true //  Ignore case differences
            });

            // Final  Check : verify the data matches what we inserted
            returnedMovie.Should().NotBeNull();
            returnedMovie.Id.Should().Be(movieId);
            returnedMovie.Title.Should().Be(expectedTitle);
        }

        [Fact]
        public async Task  GetMovieByID_ShouldReturnNotFuond_WhenMovieDoesNotExists()
        {
            var nonExistesMovieId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/movies/{nonExistesMovieId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateMovie_shouldReturnCreated_WhenDataIsValid()
        {
            // Arrange
            var requestDto = new CreateMovieDto()
            {
                Title = "The Matrix",
                Description = "A Computer hacker learns from ...",
                PosterUrl =  "this is for test",
                Genre = "Action",
                DurationInMinutes = 120 
            };

            var jsonString = JsonSerializer.Serialize(requestDto);

            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");

            // Act
            var response =  await _client.PostAsync("/api/movies", httpContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Contain("Movie Created Successfully.");
        }

        [Fact]
        public async Task UpdateMovie_ShouldReturnSuccess_WhenDataValid()
        {
            // Arrange
            var movieId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var initialMvoie = new Movie()
                {
                    Id = movieId,
                    Title = "old Title",
                    Description = "old Description",
                    Genre = "action",
                    PosterUrl =  "test",
                    DurationInMinutes = 10
                    

                };

                dbContext.Movies.Add(initialMvoie);
                await dbContext.SaveChangesAsync();
            }

            var updatedDto = new CreateMovieDto()
            {
                Title = "new Title",
                Description = "new description",
                Genre = "comedy",
                PosterUrl= "test2",
                DurationInMinutes = 15
                
            };

            var jsonString = JsonSerializer.Serialize(updatedDto);
            var httpContent = new StringContent(jsonString,Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAscheme");

            // Act
            var response = await _client.PutAsync($"/api/movies/{movieId}", httpContent);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbcContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext> ();
                var updatedMovie = await dbcContext.Movies.FindAsync(movieId);

                updatedMovie.Should().NotBeNull();
                updatedMovie.Title.Should().Be("new Title");
            }
        }

        [Fact]
        public async Task DeleteMovie_ShouldReturnSuccess_WhenMovieDeleted()
        {
            // Assert
            var movieId = Guid.NewGuid();

            //seed the  movie into the database
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Movies.Add(new Movie { Id = movieId, Title = "Movie to delete" });
                await dbContext.SaveChangesAsync();
            }

            //add fake authentication
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAScheme");

            // Act 
            var response = await _client.DeleteAsync($"/api/movies/{movieId}");

            // Assert 
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // verify the movie is completely gone from the database
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var deletedMovie = await dbContext.Movies.FindAsync(movieId);

                deletedMovie.Should().BeNull();
            }
        }

        [Fact]
        public async Task UpdatePoster_ShouldReturnSuccess_WhenValidImageIsUploaded()
        {
            // Arrange
            var movieId = Guid.NewGuid();

            // send the target movie into the in-memory database
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Movies.Add(new Movie
                {
                    Id = movieId,
                    Title = "Movie for poster",
                    Description = "for poster",
                    PosterUrl = "for test",
                    Genre = "action",
                    DurationInMinutes= 120

                });

                await dbContext.SaveChangesAsync();
            }

            // create a fake in-memory file to simulate an image upload
            var fakeImageBytes = System.Text.Encoding.UTF8.GetBytes("this is fake image data");
            var fileContent = new ByteArrayContent(fakeImageBytes);

            // tell the http clint that this byte array represents a JPEG image
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            // construct the Moltipart Form Data
            var multipartForm = new MultipartFormDataContent();

            // attach the file to the form
            multipartForm.Add(fileContent,"file", "movie_poster.jpeg");

            // apply fake authentication bypass
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Testscheme");

            // Act
            var response = await _client.PostAsync($"/api/movies/{movieId}/poster", multipartForm);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
