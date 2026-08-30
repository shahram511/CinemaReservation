using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs
{
    public class MovieCommentResponseDto
    {
        public string Id { get; set; }
        public Guid MovieId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
