using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs
{
    public class LoginUserDto
    {
        public string Username { get; set; } = string.Empty;
        
        public string Password { get; set; } = string.Empty;
    }
}
