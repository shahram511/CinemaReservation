using System;
using System.Collections.Generic;
using System.Text;
using static CinemaReservation.Core.Enums.Enums;

namespace CinemaReservation.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public UserRole Role { get; set; } = UserRole.RegularUser;

        // Navigation Properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
