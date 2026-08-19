using CinemaReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public interface IMovieRepository
    {
        Task<Movie?> GetAsync(Guid id);  // GET(id)
        Task<IEnumerable<Movie>> GetAllAsync(); // GET
        Task CreateAsync(Movie movie); // POST
        Task UpdateAsync(Movie movie); //UPDATE
        Task DeleteAsync(Movie movie); // DELETE
        Task AddShowtimeAsync(Showtime showtime);
        Task<List<Showtime>> GetShowtimesByMovieIdAsync(Guid movieId);
        Task<Showtime?> GetShowtimeByIdAsync(Guid movieId,Guid showtimId);
        //Task<Showtime?> GetShowtimeByIdAsync(Guid showtimeId);
        Task UpdateShowtimeAsync(Showtime showtime);
        Task DeleteShowtimeAsync(Showtime showtime);
    }
}
