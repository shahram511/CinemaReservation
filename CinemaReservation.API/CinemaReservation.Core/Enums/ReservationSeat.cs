using CinemaReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Enums
{
    public class ReservationSeat
    {
        public Guid ID { get; set; }
        public Guid? ReservationId { get; set; }
        public Guid SeatId { get; set; }
        public Guid ShowtimeId { get; set; }
        public decimal Price { get; set; }

        // Concurrency Token for EF Core
        public byte[] Version { get; set; } = Array.Empty<byte>();

        //public Core.Enums.Enums.ReservationStatus Status { get; set; } = Core.Enums.Enums.ReservationStatus.Pending;

        // Navigation Properties
        public Reservation? Reservation { get; set; }
        public Seat? Seat { get; set; }
    }
}
