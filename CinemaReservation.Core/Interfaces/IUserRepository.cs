using CinemaReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> UserExistAsync(string username, string email);

        Task AddUserAsync(User user);

        Task<User?> GetUserByUssernameAsync(string username);
    }
}
