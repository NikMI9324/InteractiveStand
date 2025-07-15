using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace InteractiveStand.Infrastructure.Data
{
    public class RegionDbContextFactory : IDesignTimeDbContextFactory<RegionDbContext>
    {
        public RegionDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RegionDbContext>();
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                ?? "Host=localhost;Port=5432;Database=EnergySystem;Username=postgres;Password=miroshka";
            optionsBuilder.UseNpgsql(connectionString);
            return new RegionDbContext(optionsBuilder.Options);
        }
    }
}
