using CinemaReservation.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public interface IMovieCommentService
    {
        Task<MovieCommentResponseDto> AddCommentAsync(Guid movieId, Guid userId, string userName, CreateCommentDto dto);
        Task<IEnumerable<MovieCommentResponseDto>> GetCommentsByMovieIdAsync(Guid movieId);
    }
}
