using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Entities
{
    public class MovieComment
    {
        public string Id { get; set; } =string.Empty;  // Not null(Required) 
        public Guid MovieId { get; set; }
        public Guid UserId { get; set; }
        public required string UserName { get; set; }  // required meaning  is UserName never can be null        
        public string? Text { get; set; }   //? is this can be null
        public int Rating { get; set; }  // E.g. 1 to 5 stars
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
