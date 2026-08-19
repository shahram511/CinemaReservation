using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs
{
    public class CreateMovieDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
    }
}
