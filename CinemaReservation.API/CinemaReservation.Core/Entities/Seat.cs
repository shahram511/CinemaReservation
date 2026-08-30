using CinemaReservation.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Entities
{
    public class Seat
    {
        public Guid Id { get; set; }
        public string SeatRow { get; set; } = string.Empty;
        public int SeatNumber { get; set; }

        // Navigation Properties
        public ICollection<ReservationSeat> ReservationSeats { get; set; } = new List<ReservationSeat>();
    }
}
