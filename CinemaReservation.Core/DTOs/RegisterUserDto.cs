using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.DTOs
{
    public class RegisterUserDto
    {
        public string Username { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string Password { get; set; } = string.Empty;
    }
}
