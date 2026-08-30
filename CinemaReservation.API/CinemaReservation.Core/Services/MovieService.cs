using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<IEnumerable<Movie>> GetMoviesAsync()
        {
            return await _movieRepository.GetAllAsync();
        }

        public async Task<Movie?> GetMovieAsync(Guid id)
        {
            var movie = await _movieRepository.GetAsync(id);

            if (movie == null)            
                throw new KeyNotFoundException($"Movie with ID {id} was not found");
            
            return movie;
        }

        public async Task<Movie> CreateMovieAsync(CreateMovieDto request)
        {
            var movie = new Movie
            {
                Title = request.Title,
                Description = request.Description,
                PosterUrl = request.PosterUrl,
                Genre = request.Genre,
                DurationInMinutes = request.DurationInMinutes,
            };

            await _movieRepository.CreateAsync(movie);
            return movie;
        }

        public async Task UpdateMovieAsync(Guid id,CreateMovieDto request)
        {
            var existingMovie = await _movieRepository.GetAsync(id);

            if (existingMovie == null)            
                throw new KeyNotFoundException($"Movie with ID {id} was not found");
            
            existingMovie.Title = request.Title;
            existingMovie.Description = request.Description;
            existingMovie.PosterUrl = request.PosterUrl;
            existingMovie.Genre = request.Genre;
            existingMovie.DurationInMinutes = request.DurationInMinutes;

            await _movieRepository.UpdateAsync(existingMovie);
        }

        public async Task DeleteMovieAsync(Guid id)
        {
            var movie = await _movieRepository.GetAsync(id);

            if (movie == null)           
                throw new KeyNotFoundException($"{nameof(Movie)} : ID {id} does not exist");
            
            await _movieRepository.DeleteAsync(movie);
        }

        public async Task<Showtime> AddShowTimeAsync(Guid movieId, CreateShowtimeDto request)
        {
            var movie = await _movieRepository.GetAsync(movieId);

            if (movie == null)            
                throw new KeyNotFoundException($"Movie with ID {movieId} was not found");
            
            var showTime = new Showtime()
            {
                // Id is generated automatically by Guid.NewGuid() in the Entity constructor
                MovieId=movieId, // Link the showtime to the movie
                StartTime = request.StartTime,
            };
            // Save to the database
            await _movieRepository.AddShowtimeAsync(showTime);
            return showTime;
        }

        public async Task UpdateShowtimeAsync(Guid movieId, Guid showtimeId, CreateShowtimeDto request)
        {
            var showtime = await _movieRepository.GetShowtimeByIdAsync(movieId, showtimeId);

            if (showtime == null || showtime.MovieId != movieId)            
                throw new KeyNotFoundException("Showtime not fuond for this movie");
            

            showtime.StartTime = request.StartTime;
            await _movieRepository.UpdateShowtimeAsync(showtime);
        }

        public async Task DeleteShowtimeAsync(Guid movieId, Guid showtimeId)
        {
            var showtime =await _movieRepository.GetShowtimeByIdAsync(movieId, showtimeId);

            if (showtime == null || showtime.MovieId != movieId)             
                throw new KeyNotFoundException("Showtime not found.");
            
            await _movieRepository.DeleteShowtimeAsync(showtime);
        }

        public async Task UpdatePosterUrlAsync(Guid movieId, string posterUrl)
        {
            var movie = await _movieRepository.GetAsync(movieId);

            if (movie == null)            
                throw new KeyNotFoundException($"Movie with ID {movieId} not found.");
            

            movie.PosterUrl = posterUrl;
            await _movieRepository.UpdateAsync(movie);
        }

        public async Task<List<ShowtimeResponseDto>> GetShowtimesByMovieIdAsync(Guid movieId)
        {
            var showtimes = await _movieRepository.GetShowtimesByMovieIdAsync(movieId);
            return showtimes.Select(s => new ShowtimeResponseDto
            {
                Id = s.Id,
                MovieId = s.MovieId,
                TicketPrice = s.TicketPrice,
                StartTime = s.StartTime
            }).ToList();
        }

        public async Task<ShowtimeResponseDto> GetShowtimeResponseDtoAsync(Guid movieId, Guid showtimeId)
        {
            var showtime = await _movieRepository.GetShowtimeByIdAsync(movieId, showtimeId);

            if (showtime == null)            
                throw new KeyNotFoundException("Showtime not found for this specific movie.");            

            return new ShowtimeResponseDto
            {
                Id = showtime.Id,
                MovieId = showtime.MovieId,
                StartTime = showtime.StartTime
            };
        }
    }
}
