using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs.Anlaytics
{
    public class TopCustomersDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int TotalTicketsPurchased { get; set; }
        public decimal LifeTimeValue { get; set; } // Total money spent
    }
}
