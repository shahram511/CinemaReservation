using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace CinemaReservation.Core.Enums
{
    public class Enums
    {
        public enum UserRole
        {
            RegularUser,
            Admin
        } 

        public enum ReservationStatus
        {
            Pending =0,            
            Confirmed=1,
            Cancelled =2,
        }

    }
}

