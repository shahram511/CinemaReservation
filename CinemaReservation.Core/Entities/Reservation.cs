using CinemaReservation.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using static CinemaReservation.Core.Enums.Enums;


namespace CinemaReservation.Core.Entities
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ShowtimeId { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // Navigation Properties
        public User? User { get; set; } = null; 
        public Showtime? Showtime { get; set; }= null;
        public ICollection<ReservationSeat> ReservationSeats { get; set; } = new List<ReservationSeat>();
    }
}
