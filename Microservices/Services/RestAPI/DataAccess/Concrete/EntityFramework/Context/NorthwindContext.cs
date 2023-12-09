using Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework.Context
{
    public class NorthwindContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var dbUsername = Environment.GetEnvironmentVariable("DB_NAME");
            var dbPassword = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");

            var connectionString = $@"Host={dbHost}; Database={dbName}; Username={dbUsername}; Password={dbPassword}; Port=5432; IntegratedSecurity=true; Pooling=true";

            optionsBuilder.UseNpgsql(connectionString);
        }

        public DbSet<Product> Products { get; set; }

    }
}
