using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs.Anlaytics
{
    public class CancellationImpcatDto
    {
        public Guid MovieId { get; set; }
        public string MovieTitle { get; set; }
        public int TotaledCanceledTicket { get; set; }
        public decimal LostRevenue { get; set; } // Total price of all canceled seats
    }
}
