using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs.Anlaytics
{
    public class MovieRevenueDto
    {
        public Guid MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;

        // Sales Metrics
        public int TotalTicketSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
