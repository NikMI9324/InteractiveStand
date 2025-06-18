using InteractiveStand.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStand.Infrastructure.Data
{
    public class RegionDbContext : DbContext
    {
        public DbSet<Region> Regions { get; set; }
        public DbSet<Consumer> Consumers { get; set; }
        public DbSet<PowerSource> PowerSources { get; set; }
        public DbSet<ConnectedRegion> ConnectedRegions { get; set; }
        public DbSet<PowerTransfer> PowerTransfers { get; set; }
        public RegionDbContext(DbContextOptions<RegionDbContext> options) : base(options) 
        {

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Region>()
                        .HasOne(r => r.PowerSource)
                        .WithOne()
                        .HasForeignKey<Region>(r => r.PowerSourceId)
                        .OnDelete(DeleteBehavior.Cascade); ;
            modelBuilder.Entity<Region>()
                        .HasOne(r => r.Consumer)
                        .WithOne()
                        .HasForeignKey<Region>(r => r.ConsumerId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConnectedRegion>()
                        .HasKey(cr => cr.Id);
            modelBuilder.Entity<ConnectedRegion>()
                        .HasOne<Region>()
                        .WithMany()
                        .HasForeignKey(cr => cr.RegionSourceId)
                        .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConnectedRegion>()
                        .HasOne<Region>()
                        .WithMany()
                        .HasForeignKey(cr => cr.RegionDestinationId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PowerTransfer>()
                        .HasKey(pt => pt.Id);
            modelBuilder.Entity<PowerTransfer>()
                        .HasOne<Region>()
                        .WithMany()
                        .HasForeignKey(pt => pt.WhoReceivedId)
                        .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PowerTransfer>()
                        .HasOne<Region>()
                        .WithMany()
                        .HasForeignKey(pt => pt.WhoSentId)
                        .OnDelete(DeleteBehavior.Restrict);


            List<PowerSource> powerSources = new List<PowerSource> 
            {
                new PowerSource
                {
                    Id = 1,
                    AESPercentage = 25.0,
                    GESPercentage = 4.0,
                    TESPercentage = 71.0
                },
                new PowerSource
                {
                    Id = 2,
                    AESPercentage = 15.0,
                    GESPercentage = 25.0,
                    TESPercentage = 59.0,
                    VESPercentage = 0.3,
                    SESPercentage = 0.7
                },
                new PowerSource
                {
                    Id = 3,
                    TESPercentage = 92.0,
                    GESPercentage = 4.0,
                    AESPercentage = 3.0,
                    SESPercentage = 1.0
                },
                new PowerSource
                {
                    Id = 4,
                    TESPercentage = 63.0,
                    GESPercentage = 12.0,
                    AESPercentage = 24.0,
                    VESPercentage = 1.0
                },
                new PowerSource
                {
                    Id = 5,
                    AESPercentage = 23.0,
                    GESPercentage = 15.0,
                    TESPercentage = 50.0,
                    VESPercentage = 8.0,
                    SESPercentage = 4.0
                },
                new PowerSource
                {
                    Id = 6,
                    GESPercentage = 48.0,
                    TESPercentage = 51.0,
                    SESPercentage = 1
                },
                new PowerSource
                {
                    Id = 7,
                    GESPercentage = 41.0,
                    TESPercentage = 59.0
                },
                new PowerSource
                {
                    Id = 8,
                    AESPercentage = 2.0,
                    GESPercentage = 48.0,
                    TESPercentage = 50.0
                },
                new PowerSource
                {
                    Id = 9,
                    TESPercentage = 50.0,
                    SESPercentage = 50.0
                },
                new PowerSource
                {
                    Id = 10,
                    VESPercentage = 100.0,
                }
            };
            List<Consumer> consumers = new List<Consumer> 
            {
                new Consumer { Id =  1, FirstPercentage = 30.0, SecondPercentage = 20.0, ThirdPercentage = 50.0 },
                new Consumer { Id =  2, FirstPercentage = 40.0, SecondPercentage = 20.0, ThirdPercentage = 40.0 },
                new Consumer { Id =  3, FirstPercentage =  5.0, SecondPercentage = 25.0, ThirdPercentage = 70.0 },
                new Consumer { Id =  4, FirstPercentage = 20.0, SecondPercentage = 10.0, ThirdPercentage = 70.0 },
                new Consumer { Id =  5, FirstPercentage = 20.0, SecondPercentage = 20.0, ThirdPercentage = 60.0 },
                new Consumer { Id =  6, FirstPercentage = 60.0, SecondPercentage = 20.0, ThirdPercentage = 20.0 },
                new Consumer { Id =  7, FirstPercentage = 20.0, SecondPercentage = 50.0, ThirdPercentage = 30.0 },
                new Consumer { Id =  8, FirstPercentage = 30.0, SecondPercentage = 20.0, ThirdPercentage = 50.0 },
                new Consumer { Id =  9, FirstPercentage = 70.0, SecondPercentage = 10.0, ThirdPercentage = 20.0 },
                new Consumer { Id = 10, FirstPercentage = 30.0, SecondPercentage = 20.0, ThirdPercentage = 50.0 }

            };
            List<Region> regions = new List<Region> 
            {
                new Region
                {
                    Id = 1,
                    Name = "ОЭС 1",
                    ProducedCapacity = 23.0,
                    ConsumedCapacity = 100.0,
                    PowerSourceId = 1,
                    ConsumerId = 1,
                    TimeZoneOffset = 3
                },
                new Region
                {
                    Id= 2,
                    Name = "ОЭС 2",
                    ProducedCapacity = 62.0,
                    ConsumedCapacity = 270.0,
                    PowerSourceId = 2,
                    ConsumerId = 2,
                    TimeZoneOffset = 3
                },
                new Region
                {
                    Id= 3,
                    Name = "ОЭС 3",
                    ProducedCapacity = 32.0,
                    ConsumedCapacity = 140.0,
                    PowerSourceId = 3,
                    ConsumerId = 3,
                    TimeZoneOffset = 3
                },
                new Region
                {
                    Id= 4,
                    Name = "ОЭС 4",
                    ProducedCapacity = 27.0,
                    ConsumedCapacity = 120.0,
                    PowerSourceId = 4,
                    ConsumerId = 4,
                    TimeZoneOffset = 4
                },
                new Region
                {
                    Id= 5,
                    Name = "ОЭС 5",
                    ProducedCapacity = 60.0,
                    ConsumedCapacity = 260.0,
                    PowerSourceId = 5,
                    ConsumerId = 5,
                    TimeZoneOffset = 5
                },
                new Region
                {
                    Id= 6,
                    Name = "ОЭС 6",
                    ProducedCapacity = 55.0,
                    ConsumedCapacity = 240.0,
                    PowerSourceId = 6,
                    ConsumerId = 6,
                    TimeZoneOffset = 7
                },
                new Region
                {
                    Id= 7,
                    Name = "ОЭС 7",
                    ProducedCapacity = 50.0,
                    ConsumedCapacity = 11.0,
                    PowerSourceId = 7,
                    ConsumerId = 7,
                    TimeZoneOffset = 10
                },
                new Region
                {
                    Id= 8,
                    Name = "АЭК-ТИТЭС",
                    ProducedCapacity = 15.0,
                    ConsumedCapacity = 3.0,
                    PowerSourceId = 8,
                    ConsumerId = 8,
                    TimeZoneOffset = 3
                },
                new Region
                {
                    Id= 9,
                    Name = "АЭК-ПРОМ",
                    ProducedCapacity = 15.0,
                    ConsumedCapacity = 3.0,
                    PowerSourceId = 9,
                    ConsumerId = 9,
                    TimeZoneOffset = 7
                },
                new Region
                {
                    Id= 10,
                    Name = "АЭК-ВИЭ",
                    ProducedCapacity = 10.0,
                    ConsumedCapacity = 0.5,
                    PowerSourceId = 10,
                    ConsumerId = 10,
                    TimeZoneOffset = 4
                }

            };
            List<ConnectedRegion> connectedRegions = new List<ConnectedRegion> 
            { 
                new ConnectedRegion { Id =  1, RegionSourceId =  1, RegionDestinationId =  2 },
                new ConnectedRegion { Id =  2, RegionSourceId =  1, RegionDestinationId =  5 },
                new ConnectedRegion { Id =  3, RegionSourceId =  1, RegionDestinationId =  8 },
                new ConnectedRegion { Id =  4, RegionSourceId =  2, RegionDestinationId =  1 },
                new ConnectedRegion { Id =  5, RegionSourceId =  2, RegionDestinationId =  3 },
                new ConnectedRegion { Id =  6, RegionSourceId =  2, RegionDestinationId =  4 },
                new ConnectedRegion { Id =  7, RegionSourceId =  2, RegionDestinationId =  5 },
                new ConnectedRegion { Id =  8, RegionSourceId =  3, RegionDestinationId =  2 },
                new ConnectedRegion { Id =  9, RegionSourceId =  3, RegionDestinationId =  4 },
                new ConnectedRegion { Id = 10, RegionSourceId =  4, RegionDestinationId =  2 },
                new ConnectedRegion { Id = 11, RegionSourceId =  4, RegionDestinationId =  3 },
                new ConnectedRegion { Id = 12, RegionSourceId =  4, RegionDestinationId =  5 },
                new ConnectedRegion { Id = 13, RegionSourceId =  4, RegionDestinationId = 10 },
                new ConnectedRegion { Id = 14, RegionSourceId =  5, RegionDestinationId =  1 },
                new ConnectedRegion { Id = 15, RegionSourceId =  5, RegionDestinationId =  2 },
                new ConnectedRegion { Id = 16, RegionSourceId =  5, RegionDestinationId =  4 },
                new ConnectedRegion { Id = 17, RegionSourceId =  5, RegionDestinationId =  6 },
                new ConnectedRegion { Id = 18, RegionSourceId =  6, RegionDestinationId =  5 },
                new ConnectedRegion { Id = 19, RegionSourceId =  6, RegionDestinationId =  7 },
                new ConnectedRegion { Id = 20, RegionSourceId =  6, RegionDestinationId =  9 },
                new ConnectedRegion { Id = 21, RegionSourceId =  7, RegionDestinationId =  6 },
                new ConnectedRegion { Id = 22, RegionSourceId =  8, RegionDestinationId =  1 },
                new ConnectedRegion { Id = 23, RegionSourceId =  9, RegionDestinationId =  6 },
                new ConnectedRegion { Id = 24, RegionSourceId = 10, RegionDestinationId =  4 }
            };

            modelBuilder.Entity<PowerSource>().HasData(powerSources);
            modelBuilder.Entity<Consumer>().HasData(consumers);
            modelBuilder.Entity<ConnectedRegion>().HasData(connectedRegions);
            modelBuilder.Entity<Region>().HasData(regions);

        }
    }
}
