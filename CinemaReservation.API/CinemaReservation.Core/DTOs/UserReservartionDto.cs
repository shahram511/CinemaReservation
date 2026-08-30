using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs
{
    public class ReservedSeatDto
    {
        public  Guid SeatId { get; set; }
        public string Row { get; set; } = string.Empty;
        public int Number { get; set; }
        
    }
    public class UserReservartionDto
    {
        public Guid ReservationId { get; set; }
        public Guid ShowtimeId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime ShowtimeStart { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ReservedSeatDto> Seats { get; set; } = new List<ReservedSeatDto>();
        
    }
}
