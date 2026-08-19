using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;



namespace CinemaReservation.Core.Services
{
    public class MovieCommentService : IMovieCommentService
    {
        private readonly IMovieCommentRepository _repository;

        public MovieCommentService(IMovieCommentRepository repository)
        {
            _repository = repository;
        }

        public async Task<MovieCommentResponseDto> AddCommentAsync(Guid movieId, Guid userId, string userName, CreateCommentDto dto)
        {
            var comment = new MovieComment()
            {
                MovieId = movieId,
                UserId = userId,
                UserName = userName,
                Text = dto.Text,
                Rating = dto.Rating,
                CreatedAt = DateTime.UtcNow
            };

            var savedComment = await _repository.AddCommentAsync(comment);

            return MapToResponse(savedComment);
        }

        public async Task<IEnumerable<MovieCommentResponseDto>> GetCommentsByMovieIdAsync(Guid movieId)
        {
            if (movieId == Guid.Empty)            
                throw new ArgumentException("A valid Movie ID must be provided to fetch comments.", nameof(movieId));
            

            // 2. Fetch from Database
            var comments = await _repository.GetCommentsByMovieIdAsync(movieId);

            // 3. Map to DTOs and return
            return comments.Select(c => new MovieCommentResponseDto
            {
                Id = c.Id,
                MovieId = c.MovieId,
                UserName = c.UserName, 
                Text = c.Text,
                Rating = c.Rating,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        private static  MovieCommentResponseDto MapToResponse(MovieComment comment)
        {
            return new MovieCommentResponseDto()
            {
                Id = comment.Id,
                MovieId = comment.MovieId,
                UserName = comment.UserName,
                Text = comment.Text,
                Rating = comment.Rating,
                CreatedAt = comment.CreatedAt,

            };
        }
    }
}
