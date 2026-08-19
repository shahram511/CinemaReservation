using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs
{
    public class SeatAvailabilityDto
    {
        public Guid Id { get; set; }
        public string Row { get; set; } = string.Empty;
        public int Number { get; set; }
        public bool IsAvailable { get; set; }
    }
}
