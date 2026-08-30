using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Entities
{
    public class Movie
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; } 

        // Navigation Properties
        public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}
