using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs
{
    public class ShowtimeResponseDto
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public decimal TicketPrice { get; set; }
        public DateTime StartTime { get; set; }
    }
}
