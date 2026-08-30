using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Infrastructure.Repositories
{
    public class UserRepositoriy :IUserRepository
    {
        private readonly ApplicationDbContext _userRepository;
        public UserRepositoriy(ApplicationDbContext userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task AddUserAsync(User user)
        {
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
               
   
        }

        public async Task<User?> GetUserByUssernameAsync(string username)
        {
            return await _userRepository.Users.FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<bool> UserExistAsync(string username, string email)
        {
            return await _userRepository.Users.AnyAsync(u => u.Username == username || u.Email == email);
        }
    }
}
