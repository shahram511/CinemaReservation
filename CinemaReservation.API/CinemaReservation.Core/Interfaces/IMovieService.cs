using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public interface IMovieService
    {
        Task<Movie?> GetMovieAsync(Guid id);  // GET(id)
        Task<IEnumerable<Movie>> GetMoviesAsync(); // GET
        Task<Movie> CreateMovieAsync(CreateMovieDto request); // POST
        Task UpdateMovieAsync(Guid id,CreateMovieDto request); //UPDATE
        Task DeleteMovieAsync(Guid id); // DELETE
        Task<Showtime> AddShowTimeAsync(Guid movieId, CreateShowtimeDto request);
        Task<List<ShowtimeResponseDto>> GetShowtimesByMovieIdAsync(Guid movieId);
        Task<ShowtimeResponseDto> GetShowtimeResponseDtoAsync(Guid movieId, Guid showtimeId);
        Task UpdateShowtimeAsync(Guid movieId,Guid showtimeId, CreateShowtimeDto request);
        Task DeleteShowtimeAsync(Guid movieId,Guid showtimeId);

        // we need a quik method to update the URL string in database after the fiel is saved
        Task UpdatePosterUrlAsync(Guid movieId, string posterUrl);

    }
}
