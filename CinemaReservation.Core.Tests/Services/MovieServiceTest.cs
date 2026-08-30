using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Core.Services;
using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Tests.Services
{
    public class MovieServiceTest
    {
        private readonly Mock<IMovieRepository> _movieRepositoryMock;
        private readonly MovieService _movieService;

        public MovieServiceTest() // constructor should be empty
        {

            _movieRepositoryMock = new Mock<IMovieRepository>(); // make objects with new in the constructor
            _movieService = new MovieService(_movieRepositoryMock.Object); // pas the Mock to the service
        }

        [Fact]
        public async Task GetMovieAsync_ShouldReturnMovie_WhenMovieExists()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var expectedMovie = new Movie { Id = movieId, Title = "inception" };

            _movieRepositoryMock
                .Setup(repo => repo.GetAsync(movieId))
                .ReturnsAsync(expectedMovie);

            // Act
            var result = await _movieService.GetMovieAsync(movieId);

            // Assert 
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedMovie);
        }
        [Fact]
        public async Task GetMovieAsync_shouldThrowException_WhenMovieIsNull()
        {
            // Arrang
            var movieId = Guid.NewGuid();

            _movieRepositoryMock
                .Setup(repo => repo.GetAsync(movieId))
                .ReturnsAsync((Movie?) null);

            // Act
            Func<Task> action = async () => await _movieService.GetMovieAsync(movieId);

            // Assert
            await action.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Movie with ID {movieId} was not found");
        }
    }
}
