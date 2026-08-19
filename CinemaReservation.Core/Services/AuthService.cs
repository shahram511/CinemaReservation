using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CinemaReservation.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        public AuthService(IUserRepository userRepository,IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<string> LoginAsync(string username, string plainTextPassword)
        {
            var user = await _userRepository.GetUserByUssernameAsync(username);

            if (user == null)            
                throw new Exception("Invalid username or password");
            

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(plainTextPassword, user.PasswordHash);

            if (!isPasswordCorrect)            
                throw new Exception("Invalid username or password!");
            

            string secretKey = _configuration["JwtSettings:Secret"] ?? throw new Exception("jwt token is missing!");
            return GenerateJwtToken(user, secretKey);

        }

        private string GenerateJwtToken(User user, string secretKey)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // convert the secret key string into a cryptographic byte array
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Choose the string algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Construct the token with an expiration time
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
                );

            // Serialize the token into the final string format
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task RegisterUserAsync(string username,string email, string plainTextPassword)
        {
            bool userExisting = await _userRepository.UserExistAsync(username,email);

            if (userExisting)            
                throw new Exception("username or email already taken !!");
           

            string hashesPassword = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

            var newUser = new User()
            {
                Username = username,
                Email = email,
                PasswordHash = hashesPassword
            };

            await _userRepository.AddUserAsync(newUser);
        }
    }
}
