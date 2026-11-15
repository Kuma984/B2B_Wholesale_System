using System.Threading.Tasks;
using B2B.Core.Data;
using B2B.Core.Entities;
using B2B.Core.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace B2B.Tests
{
    public class AuthServiceTests
    {
        private B2BDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<B2BDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new B2BDbContext(options);
        }

        [Fact]
        public async Task Authenticate_ValidAdmin_ReturnsSuccess()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("Auth_Admin_Success");
            var service = new AuthService(context);
            var user = new User
            {
                UserName = "admin_test",
                Email = "admin@test.com",
                Password = service.HashPassword("AdminPass123!"),
                Role = "Admin",
                Lock = false
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await service.AuthenticateAsync("admin_test", "AdminPass123!", "Admin");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal("Admin", result.User.Role);
        }

        [Fact]
        public async Task Authenticate_WrongRole_RejectsAccess()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("Auth_WrongRole_Db");
            var service = new AuthService(context);
            var agentUser = new User
            {
                UserName = "agent_test",
                Email = "agent@test.com",
                Password = service.HashPassword("AgentPass123!"),
                Role = "Agent",
                Lock = false
            };
            context.Users.Add(agentUser);
            await context.SaveChangesAsync();

            // Act: Agent tries to log into WinForms Admin app
            var result = await service.AuthenticateAsync("agent_test", "AgentPass123!", "Admin");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Access denied", result.Message);
        }

        [Fact]
        public async Task Authenticate_LockedAccount_RejectsAccess()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("Auth_LockedAccount_Db");
            var service = new AuthService(context);
            var lockedUser = new User
            {
                UserName = "locked_user",
                Email = "locked@test.com",
                Password = service.HashPassword("Password123!"),
                Role = "Agent",
                Lock = true // Account is locked!
            };
            context.Users.Add(lockedUser);
            await context.SaveChangesAsync();

            // Act
            var result = await service.AuthenticateAsync("locked_user", "Password123!", "Agent");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("locked", result.Message);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(2, false)]
        [InlineData(3, true)]
        [InlineData(4, true)]
        public void ShouldLockAccount_ConsecutiveFailedAttempts_EnforcesThreshold(int attempts, bool expectedLock)
        {
            // Arrange
            using var context = CreateInMemoryDbContext("Auth_LockoutThreshold_Db");
            var service = new AuthService(context);

            // Act
            var isLocked = service.ShouldLockAccount(attempts, maxAllowedAttempts: 3);

            // Assert
            Assert.Equal(expectedLock, isLocked);
        }
    }
}
