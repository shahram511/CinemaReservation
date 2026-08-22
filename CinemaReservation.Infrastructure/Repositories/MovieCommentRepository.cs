using CinemaReservation.Core.DTOs.Anlaytics;
using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Infrastructure.Data;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CinemaReservation.Infrastructure.Repositories
{
    public class MovieCommentRepository : IMovieCommentRepository
    {
        private readonly IMongoCollection<MovieComment> _movieComments;

        public MovieCommentRepository(MongoContext context)
        {
            _movieComments = context.MovieComments;
        }

        public async Task<MovieComment> AddCommentAsync(MovieComment comment)
        {
            await _movieComments.InsertOneAsync(comment);
            return comment;
        }

        public async Task<IEnumerable<MovieComment>> GetCommentsByMovieIdAsync(Guid movieId)
        {
            var filter  = Builders<MovieComment>.Filter.Eq(c =>  c.MovieId, movieId);

            var result= await  _movieComments.Find(filter)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();

            return result;
        }

        public async Task<MovieEngagmentDto> GetCommentsInfoByMovieIdRepoAsync(Guid movieId)
        {
            var comments = await GetCommentsByMovieIdAsync(movieId);

            if (!comments.Any())
                return new MovieEngagmentDto { MovieId = movieId, TotalComments = 0, AvrageRate = 0 };

                
            return new MovieEngagmentDto()
            {
                MovieId = movieId,
                TotalComments = comments.Count(),
                AvrageRate = Math.Round(comments.Average(c => c.Rating),1)
            };
        }
    }
}
