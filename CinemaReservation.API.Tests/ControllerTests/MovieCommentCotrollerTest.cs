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

namespace CinemaReservation.API.Tests.ControllerTests
{
    public class MovieCommentCotrollerTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        public MovieCommentCotrollerTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task AddComment_WithValidDataToken_ReturnsCreated()
        {
            var mockRepo = _factory.Services.GetRequiredService<Mock<IMovieCommentRepository>>();
            var requestDto = new CreateCommentDto { Text = "123" ,Rating = 5};
            var testUserId= Guid.NewGuid();
            var testMovieId = Guid.NewGuid();


            var fakeCommentEntitiy = new MovieComment()
            {
                Id =MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                MovieId = testMovieId,
                UserName = "TestUser",
                Text = requestDto.Text,
                Rating = requestDto.Rating,               
            };

            mockRepo.Setup(repo => repo.AddCommentAsync(It.IsAny<MovieComment>()))
                .ReturnsAsync(fakeCommentEntitiy);

            var jsonString = JsonSerializer.Serialize(requestDto);

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");            

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");

            var response = await _client.PostAsync($"/api/movie/{testMovieId}/MovieComment", content);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // verify that the mock repo's add method was executed exactly once during the API call
            mockRepo.Verify(repo => repo.AddCommentAsync(It.IsAny<MovieComment>()), Times.Once);
        }

        [Fact]
        public async Task GetComment_ReturnsOk_WithMockedData()
        {
            var mockRepo = _factory.Services.GetRequiredService<Mock<IMovieCommentRepository>>();

            var testMovieId = Guid.NewGuid();
            var fakeComments = new List<MovieComment>()
            {
                new MovieComment()
                {
                    MovieId = testMovieId,
                    UserName = "test",
                    Text = "good",
                    Rating = 5
                }
            };

            mockRepo.Setup(repo => repo.GetCommentsByMovieIdAsync(testMovieId)).ReturnsAsync(fakeComments);

            var response = await _client.GetAsync($"/api/movie/{testMovieId}/moviecomment");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            mockRepo.Verify(repo => repo.GetCommentsByMovieIdAsync(testMovieId), Times.Once);
        }

        [Fact]
        public async Task GetNumberCommentsAverageRate_ReturnsOk_WithAnalyticsData()
        {
            var mockRepo = _factory.Services.GetRequiredService<Mock<IMovieCommentRepository>>();

            var testMovieId = Guid.NewGuid();

            var fakeAnalytics = new MovieEngagmentDto()
            {
                TotalComments = 120,
                AvrageRate = 4
            };

            mockRepo.Setup(repo => repo.GetCommentsInfoByMovieIdRepoAsync(testMovieId)).ReturnsAsync(fakeAnalytics);

            var response = await _client.GetAsync($"/api/movie/{testMovieId}/moviecomment/info");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            mockRepo.Verify(repo => repo.GetCommentsInfoByMovieIdRepoAsync(testMovieId), Times.Once);
        }

    }
}
