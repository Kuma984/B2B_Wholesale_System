using System.Threading.Tasks;
using B2B.Core.Entities;

namespace B2B.Core.Services
{
    public interface IAuthService
    {
        string HashPassword(string plainPassword);
        Task<(bool Success, string Message, User? User)> AuthenticateAsync(string userNameOrEmail, string password, string expectedRole);
        bool ShouldLockAccount(int consecutiveFailedAttempts, int maxAllowedAttempts = 3);
    }
}
