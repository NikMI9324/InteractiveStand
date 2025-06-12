using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace InteractiveStand.Infrastructure.Data
{
    public class RegionDbContextFactory : IDesignTimeDbContextFactory<RegionDbContext>
    {
        public RegionDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RegionDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=EnergySystem;Username=postgres;" +
                "Password=miroshka;Timeout=10;SslMode=Prefer");
            return new RegionDbContext(optionsBuilder.Options);
        }
    }
}
