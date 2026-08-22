using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.DTOs.Anlaytics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public interface IMovieCommentService
    {
        Task<MovieCommentResponseDto> AddCommentAsync(Guid movieId, Guid userId, string userName, CreateCommentDto dto);
        Task<MovieEngagmentDto> GetCommentInfoByMovieIdAsync(Guid movieId);
        Task<IEnumerable<MovieCommentResponseDto>> GetCommentsByMovieIdAsync(Guid movieId);
    }
}
