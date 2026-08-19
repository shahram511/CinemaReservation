using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public  interface IAuthService
    {
        Task RegisterUserAsync(string username,  string email, string plainTextPassword);

        Task<string> LoginAsync(string username, string plainTextPassword);        
    }
}
