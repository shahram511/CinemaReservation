using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs.Anlaytics
{
    public class MovieEngagmentDto
    {
        public Guid MovieId { get; set; }
        public int TotalComments { get; set; }
        public double AvrageRate { get; set; }
    }
}
