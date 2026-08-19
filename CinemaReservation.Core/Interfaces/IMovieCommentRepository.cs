using CinemaReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public interface IMovieCommentRepository
    {
        Task<MovieComment> AddCommentAsync(MovieComment comment);
        Task<IEnumerable<MovieComment>>  GetCommentsByMovieIdAsync(Guid movieId);
    }
}