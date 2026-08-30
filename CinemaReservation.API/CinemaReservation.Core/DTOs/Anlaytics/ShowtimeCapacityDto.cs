using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs.Anlaytics
{
    public class ShowtimeCapacityDto
    {
        public Guid ShowtimeId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }

        // Capacity Metrics
        public int TotalSeats { get; set; }
        public int ReservedSeats { get; set; }
        public int AvailableSeats { get; set; }

        // Business Metric: percentage of seates sold
        public double OccupancyRatePercentage { get; set; }
    }
}
