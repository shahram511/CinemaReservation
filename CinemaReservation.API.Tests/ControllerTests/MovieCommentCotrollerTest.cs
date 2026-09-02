using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net;
using CinemaReservation.Core.Entities;
using CinemaReservation.Core.DTOs.Anlaytics;
using CinemaReservation.Infrastructure.Data;
using MongoDB.Driver;
using System.Net.Http.Json;

namespace CinemaReservation.API.Tests.ControllerTests
{
    [Collection("SharedDatabaseCollection")]
    public class MovieCommentCotrollerTest : IntegrationTestBase
    {
        public MovieCommentCotrollerTest(SharedDatabaseFixture fixture) : base(fixture)
        {
        }


        [Fact]
        public async Task AddComment_WithValidDataToken_ReturnsCreated()
        {
            var testMovieId = Guid.NewGuid();
            var requestDto = new CreateCommentDto { Text = "123" ,Rating = 5};


            // Arrange 
            using var scope = Factory.Services.CreateScope();
            var postgressContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            postgressContext.Movies.Add(new Movie
            {
                Id = testMovieId,
                Title = "test",
                Description = "test",
                Genre = "test",
                PosterUrl = "test",
                DurationInMinutes = 10
            });

            await postgressContext.SaveChangesAsync();            

            var jsonString = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");    
            
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");

            // Act : excute the actual HTTP request
            var response = await Client.PostAsync($"/api/movie/{testMovieId}/MovieComment", content);

            // Assert : chcke the HTTP response
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // verify database state: Query the real MongoDB container
            var mongoContext = scope.ServiceProvider.GetRequiredService<MongoContext>();

            var savedComment = await mongoContext.MovieComments
                .Find(c => c.MovieId == testMovieId && c.Text == requestDto.Text)
                .FirstOrDefaultAsync();

            savedComment.Should().NotBeNull();
            savedComment.Rating.Should().Be(5);
            savedComment.UserName.Should().NotBeNullOrEmpty();
            savedComment.Text.Should().Be("123");

        }

        [Fact]
        public async Task GetComment_ReturnsOk_WithSeededData()
        {
            // Arrange : prepare the environment before the test runs                       
            var testMovieId = Guid.NewGuid();

            // create a dependency injection scope to resolve our database contexts
            using var scope = Factory.Services.CreateScope();

            var postgresContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            postgresContext.Movies.Add(new Movie()
            {
                Id = testMovieId,
                Title = "test",
                Description = "for test",
                PosterUrl = "test",
                Genre = "action",
                DurationInMinutes = 10
            });

            // physically save the movie to the postgreSQL Testcontainer
            await postgresContext.SaveChangesAsync();

            var mongoContext = scope.ServiceProvider.GetRequiredService<MongoContext>();

            var mongoComment = new MovieComment()
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                MovieId = testMovieId,
                UserName = "shahram",
                Text = "for test",
                Rating = 4

            };

            await mongoContext.MovieComments.InsertOneAsync(mongoComment);

            // Act : excute the endponit 
            var response = await Client.GetAsync($"/api/movie/{testMovieId}/MovieComment");

            // Assert 
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var returnedComments = await response.Content.ReadFromJsonAsync<List<MovieCommentResponseDto>>();

            returnedComments.Should().NotBeNullOrEmpty();

            // verify that the data returned from the API matches exactly what we seeded in MongoDB
            var retrievedComment = returnedComments.First();
            retrievedComment.Text.Should().Be("for test");
            retrievedComment.Rating.Should().Be(4);
            retrievedComment.UserName.Should().Be("shahram");           
        }

        [Fact]
        public async Task GetNumberCommentsAverageRate_ReturnsOk_WithAnalyticsData()
        {
            // Arrange
            var testMovieId = Guid.NewGuid();
            using var scope = Factory.Services.CreateScope();

            // seed the parent movie in the PostgreSQL
            var postgresContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            postgresContext.Movies.Add(new Movie()
            {
                Id = testMovieId,
                Title = " inception",
                Description = "test",
                PosterUrl = "for test",
                Genre = "action",
                DurationInMinutes = 10
            });

            await postgresContext.SaveChangesAsync();

            var mongoContext = scope.ServiceProvider.GetRequiredService<MongoContext>();

            var comment1 = new MovieComment()
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                MovieId = testMovieId,
                UserName = "shahram",
                Text = "for test1",
                Rating = 5 

            };
            
            var comment2 = new MovieComment()
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                MovieId = testMovieId,
                UserName = "bahram",
                Text = "for test2",
                Rating = 3
            };

            await mongoContext.MovieComments.InsertManyAsync(new[] { comment1, comment2 });

            // Act :  call the api
            var response = await Client.GetAsync($"/api/movie/{testMovieId}/MovieComment/info");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // deserialize the JSON response back into the Dto
            var analyticData = await response.Content.ReadFromJsonAsync<MovieEngagmentDto>();

            analyticData.Should().NotBeNull();
            analyticData.TotalComments.Should().Be(2);
            analyticData.AvrageRate.Should().Be(4);
        }
    }
}
