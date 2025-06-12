using InteractiveStand.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Runtime.CompilerServices;

namespace InteractiveStand.Infrastructure.Data
{
    internal class RegionDbContext : DbContext
    {
        public DbSet<Region> Regions { get; set; }
        public DbSet<Consumer> Consumers { get; set; }
        public DbSet<PowerSource> PowerSources { get; set; }
        public DbSet<ConnectedRegion> ConnectedRegions { get; set; }
        public RegionDbContext(DbContextOptions<RegionDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Region>()
                        .HasOne<PowerSource>()
                        .WithOne()
                        .HasForeignKey<Region>(r => r.PowerSourceId);
            modelBuilder.Entity<Consumer>()
                        .HasOne<Consumer>()
                        .WithOne()
                        .HasForeignKey<Region>(r => r.ConsumerId);
            modelBuilder.Entity<ConnectedRegion>()
                        .HasKey(cr => cr.Id);
            modelBuilder.Entity<PowerSource>().HasData(new PowerSource
            {
                Id = 1,

            });
            modelBuilder.Entity<OES>().HasData(new OES
            {
                Id = 1,
                Name = "ОЭС 1",
                ProducedCapacity = 23.0,
                ConsumedCapacity = 100.0,
                PowerSourceId = 1,
                ConsumerId = 1,
            });
        }
    }
}
