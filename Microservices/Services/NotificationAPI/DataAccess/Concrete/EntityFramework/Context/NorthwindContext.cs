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

            var connectionString = $@"Host=localhost; Database=notificationDB; Username=postgres; Password=1234; Port=5432; IntegratedSecurity=true; Pooling=true";

            optionsBuilder.UseNpgsql(connectionString);
        }

        public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }

    }
}
