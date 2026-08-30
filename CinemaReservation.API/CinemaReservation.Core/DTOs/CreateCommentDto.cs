using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs
{
    public class CreateCommentDto
    {
        public string Text { get; set; }
        public int  Rating { get; set; }
    }
}
