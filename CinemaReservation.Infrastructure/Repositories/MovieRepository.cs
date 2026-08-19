using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly ApplicationDbContext _context;
        public MovieRepository(ApplicationDbContext context)
        {
            _context = context;            
        }

        public async Task CreateAsync(Movie movie)
        {
            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Movie movie)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }

        public async Task<Movie?> GetAsync(Guid id)
        {
            return await _context.Movies.Include(m => m.Showtimes).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            return await _context.Movies.Include(m => m.Showtimes).AsNoTracking().ToListAsync();
        }

        public async Task UpdateAsync(Movie movie)
        {
            
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }

        public async Task AddShowtimeAsync(Showtime showtime)
        {
            //adds the showtime directly in to the Showtimes table
            await _context.Showtimes.AddAsync(showtime);
            await _context.SaveChangesAsync();
        }

        //public async Task<Showtime?> GetShowtimeByIdAsync(Guid showtimeId)
        //{
            //return await _context.Showtimes.FirstOrDefaultAsync(s => s.Id == showtimeId);
        //}

        public async Task UpdateShowtimeAsync(Showtime showtime)
        {
            _context.Showtimes.Update(showtime);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteShowtimeAsync(Showtime showtime)
        {
            _context.Showtimes.Remove(showtime);
            await _context.SaveChangesAsync();
                
        }

        public async Task<List<Showtime>> GetShowtimesByMovieIdAsync(Guid movieId)
        {
            return await _context.Showtimes.Where(s => s.MovieId == movieId).OrderBy(s => s.StartTime).ToListAsync();
        }

        public async Task<Showtime?> GetShowtimeByIdAsync(Guid movieId, Guid showtimId)
        {
            return await _context.Showtimes.FirstOrDefaultAsync(s => s.MovieId== movieId && s.Id== showtimId);
        }    
    }
}
