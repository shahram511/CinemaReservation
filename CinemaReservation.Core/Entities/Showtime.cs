using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Entities
{
    public class Showtime
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public DateTime StartTime { get; set; }        

        // Navigation Properties
        public Movie? Movie { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
