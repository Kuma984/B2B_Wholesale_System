using System;
using System.Configuration;
using B2B.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.WinForms
{
    public static class DbContextFactory
    {
        public static string GetConnectionString()
        {
            var setting = ConfigurationManager.ConnectionStrings["B2BDatabase"];
            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
            {
                throw new InvalidOperationException("Connection string 'B2BDatabase' was not found in App.config.");
            }
            return setting.ConnectionString;
        }

        public static B2BDbContext CreateDbContext()
        {
            var connStr = GetConnectionString();
            var options = new DbContextOptionsBuilder<B2BDbContext>()
                .UseSqlServer(connStr)
                .Options;

            return new B2BDbContext(options);
        }
    }
}
