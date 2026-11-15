using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using B2B.Core.Data;
using B2B.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2B.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly B2BDbContext _context;

        public AuthService(B2BDbContext context)
        {
            _context = context;
        }

        public string HashPassword(string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword)) return string.Empty;
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(plainPassword);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public bool ShouldLockAccount(int consecutiveFailedAttempts, int maxAllowedAttempts = 3)
        {
            return consecutiveFailedAttempts >= maxAllowedAttempts;
        }

        public async Task<(bool Success, string Message, User? User)> AuthenticateAsync(
            string userNameOrEmail, 
            string password, 
            string expectedRole)
        {
            if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Username and password are required.", null);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.UserName == userNameOrEmail || u.Email == userNameOrEmail);

            if (user == null)
            {
                return (false, "Invalid credentials.", null);
            }

            if (user.Lock)
            {
                return (false, "This account is currently locked. Please contact system administrator.", null);
            }

            // Check plain password or hashed password
            bool passwordMatches = user.Password == password || user.Password == HashPassword(password);
            if (!passwordMatches)
            {
                return (false, "Invalid credentials.", null);
            }

            // Check Role
            if (!string.Equals(user.Role, expectedRole, StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Access denied. Account does not have '{expectedRole}' privileges.", null);
            }

            return (true, "Authentication successful.", user);
        }
    }
}
