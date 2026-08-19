using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Infrastructure.Data;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
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
    }
}
