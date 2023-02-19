using FrogAlert.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FrogAlert.Database
{
    public class FrogAlertDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public FrogAlertDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<EnvironmentSnapshot> Environment => Set<EnvironmentSnapshot>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var connectionString = _configuration.GetSection("ConnectionStrings").GetValue<string>("FrogAlertDatabase");

            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
