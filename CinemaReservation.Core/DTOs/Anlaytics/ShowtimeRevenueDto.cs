using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs.Anlaytics
{
    public class ShowtimeRevenueDto
    {
        public Guid ShowtimeId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }

        // Sales Metrics
        public int TicketSold { get; set; }
        public decimal ShowtimeRevenue { get; set; }
    }
}
