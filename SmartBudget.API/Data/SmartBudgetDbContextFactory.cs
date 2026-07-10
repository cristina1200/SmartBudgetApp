using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SmartBudget.Server.Data;
using System.IO;

namespace SmartBudget.Server.Data
{
    public class SmartBudgetDbContextFactory : IDesignTimeDbContextFactory<SmartBudgetDbContext>
    {
        public SmartBudgetDbContext CreateDbContext(string[] args)
        {
            // Setăm calea către appsettings.json pentru a lua Connection String-ul
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<SmartBudgetDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            return new SmartBudgetDbContext(builder.Options);
        }
    }
}