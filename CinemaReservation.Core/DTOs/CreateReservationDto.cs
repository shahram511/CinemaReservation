using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs
{
    public class CreateReservationDto
    {
        public Guid ShowtimeId { get; set; }

        public List<Guid> SeatIds { get; set; } = new List<Guid>();
    }
}
