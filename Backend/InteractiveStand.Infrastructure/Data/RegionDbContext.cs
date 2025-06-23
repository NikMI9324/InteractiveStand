using InteractiveStand.Domain.Classes;
using InteractiveStand.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStand.Infrastructure.Data
{
    public class RegionDbContext : DbContext
    {
        public DbSet<Region> Regions { get; set; }
        public DbSet<Consumer> Consumers { get; set; }
        public DbSet<PowerSource> PowerSources { get; set; }
        public DbSet<ConnectedRegion> ConnectedRegions { get; set; }
        public DbSet<ConsumerBinding> ConsumerBindings { get; set; }
        public DbSet<ProducerBinding> ProducerBindings { get; set; }
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

            modelBuilder.Entity<ProducerBinding>()
                        .HasKey(pb => pb.Id);
            modelBuilder.Entity<ProducerBinding>()
                        .HasOne(pb => pb.Region)
                        .WithMany()
                        .HasForeignKey(pb => pb.RegionId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConsumerBinding>()
                        .HasKey(cb => cb.Id);
            modelBuilder.Entity<ConsumerBinding>()
                        .HasOne(cb => cb.Region)
                        .WithMany()
                        .HasForeignKey(cb => cb.RegionId)
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
            List<ProducerBinding> producerBindings = new List<ProducerBinding>() 
            { 
                new ProducerBinding { Id =  1, CapacityProducerType = CapacityProducerType.TES, RegionId =  1 },
                new ProducerBinding { Id =  2, CapacityProducerType = CapacityProducerType.GES, RegionId =  1 },
                new ProducerBinding { Id =  3, CapacityProducerType = CapacityProducerType.AES, RegionId =  1 },
                new ProducerBinding { Id =  4, CapacityProducerType = CapacityProducerType.VES, RegionId =  1 },
                new ProducerBinding { Id =  5, CapacityProducerType = CapacityProducerType.SES, RegionId =  1 },
                new ProducerBinding { Id =  6, CapacityProducerType = CapacityProducerType.TES, RegionId =  2 },
                new ProducerBinding { Id =  7, CapacityProducerType = CapacityProducerType.GES, RegionId =  2 },
                new ProducerBinding { Id =  8, CapacityProducerType = CapacityProducerType.AES, RegionId =  2 },
                new ProducerBinding { Id =  9, CapacityProducerType = CapacityProducerType.VES, RegionId =  2 },
                new ProducerBinding { Id = 10, CapacityProducerType = CapacityProducerType.SES, RegionId =  2 },
                new ProducerBinding { Id = 11, CapacityProducerType = CapacityProducerType.TES, RegionId =  3 },
                new ProducerBinding { Id = 12, CapacityProducerType = CapacityProducerType.GES, RegionId =  3 },
                new ProducerBinding { Id = 13, CapacityProducerType = CapacityProducerType.AES, RegionId =  3 },
                new ProducerBinding { Id = 14, CapacityProducerType = CapacityProducerType.VES, RegionId =  3 },
                new ProducerBinding { Id = 15, CapacityProducerType = CapacityProducerType.SES, RegionId =  3 },
                new ProducerBinding { Id = 16, CapacityProducerType = CapacityProducerType.TES, RegionId =  4 },
                new ProducerBinding { Id = 17, CapacityProducerType = CapacityProducerType.GES, RegionId =  4 },
                new ProducerBinding { Id = 18, CapacityProducerType = CapacityProducerType.AES, RegionId =  4 },
                new ProducerBinding { Id = 19, CapacityProducerType = CapacityProducerType.VES, RegionId =  4 },
                new ProducerBinding { Id = 20, CapacityProducerType = CapacityProducerType.SES, RegionId =  4 },
                new ProducerBinding { Id = 21, CapacityProducerType = CapacityProducerType.TES, RegionId =  5 },
                new ProducerBinding { Id = 22, CapacityProducerType = CapacityProducerType.GES, RegionId =  5 },
                new ProducerBinding { Id = 23, CapacityProducerType = CapacityProducerType.AES, RegionId =  5 },
                new ProducerBinding { Id = 24, CapacityProducerType = CapacityProducerType.VES, RegionId =  5 },
                new ProducerBinding { Id = 25, CapacityProducerType = CapacityProducerType.SES, RegionId =  6 },
                new ProducerBinding { Id = 26, CapacityProducerType = CapacityProducerType.TES, RegionId =  6 },
                new ProducerBinding { Id = 27, CapacityProducerType = CapacityProducerType.GES, RegionId =  6 },
                new ProducerBinding { Id = 28, CapacityProducerType = CapacityProducerType.AES, RegionId =  6 },
                new ProducerBinding { Id = 29, CapacityProducerType = CapacityProducerType.VES, RegionId =  6 },
                new ProducerBinding { Id = 30, CapacityProducerType = CapacityProducerType.SES, RegionId =  6 },
                new ProducerBinding { Id = 31, CapacityProducerType = CapacityProducerType.TES, RegionId =  7 },
                new ProducerBinding { Id = 32, CapacityProducerType = CapacityProducerType.GES, RegionId =  7 },
                new ProducerBinding { Id = 33, CapacityProducerType = CapacityProducerType.AES, RegionId =  7 },
                new ProducerBinding { Id = 34, CapacityProducerType = CapacityProducerType.VES, RegionId =  7 },
                new ProducerBinding { Id = 35, CapacityProducerType = CapacityProducerType.SES, RegionId =  7 },
                new ProducerBinding { Id = 36, CapacityProducerType = CapacityProducerType.TES, RegionId =  8 },
                new ProducerBinding { Id = 37, CapacityProducerType = CapacityProducerType.GES, RegionId =  8 },
                new ProducerBinding { Id = 38, CapacityProducerType = CapacityProducerType.AES, RegionId =  8 },
                new ProducerBinding { Id = 39, CapacityProducerType = CapacityProducerType.VES, RegionId =  8 },
                new ProducerBinding { Id = 40, CapacityProducerType = CapacityProducerType.SES, RegionId =  8 },
                new ProducerBinding { Id = 41, CapacityProducerType = CapacityProducerType.TES, RegionId =  9 },
                new ProducerBinding { Id = 42, CapacityProducerType = CapacityProducerType.GES, RegionId =  9 },
                new ProducerBinding { Id = 43, CapacityProducerType = CapacityProducerType.AES, RegionId =  9 },
                new ProducerBinding { Id = 44, CapacityProducerType = CapacityProducerType.VES, RegionId =  9 },
                new ProducerBinding { Id = 45, CapacityProducerType = CapacityProducerType.SES, RegionId =  9 },
                new ProducerBinding { Id = 46, CapacityProducerType = CapacityProducerType.TES, RegionId = 10 },
                new ProducerBinding { Id = 47, CapacityProducerType = CapacityProducerType.GES, RegionId = 10 },
                new ProducerBinding { Id = 48, CapacityProducerType = CapacityProducerType.AES, RegionId = 10 },
                new ProducerBinding { Id = 49, CapacityProducerType = CapacityProducerType.VES, RegionId = 10 },
                new ProducerBinding { Id = 50, CapacityProducerType = CapacityProducerType.SES, RegionId = 10 }
            };
            List<ConsumerBinding> consumerBindings = new List<ConsumerBinding>() 
            {
                new ConsumerBinding{ Id =  1, CapacityConsumerType =     CapacityConsumerType.FirstCategory, RegionId =  1 },
                new ConsumerBinding{ Id =  2, CapacityConsumerType = CapacityConsumerType.RemainingCategory, RegionId =  1 },
                new ConsumerBinding{ Id =  3, CapacityConsumerType =     CapacityConsumerType.FirstCategory, RegionId =  2 },
                new ConsumerBinding{ Id =  4, CapacityConsumerType = CapacityConsumerType.RemainingCategory, RegionId =  2 },
                new ConsumerBinding{ Id =  5, CapacityConsumerType =     CapacityConsumerType.FirstCategory, RegionId =  3 },
                new ConsumerBinding{ Id =  6, CapacityConsumerType = CapacityConsumerType.RemainingCategory, RegionId =  3 },
                new ConsumerBinding{ Id =  7, CapacityConsumerType =     CapacityConsumerType.FirstCategory, RegionId =  4 },
                new ConsumerBinding{ Id =  8, CapacityConsumerType = CapacityConsumerType.RemainingCategory, RegionId =  4 },
                new ConsumerBinding{ Id =  9, CapacityConsumerType =     CapacityConsumerType.FirstCategory, RegionId =  5 },
                new ConsumerBinding{ Id = 10, CapacityConsumerType = CapacityConsumerType.RemainingCategory, RegionId =  5 },
                new ConsumerBinding{ Id = 11, CapacityConsumerType =     CapacityConsumerType.FirstCategory, RegionId =  6 },
                new ConsumerBinding{ Id = 12, CapacityConsumerType = CapacityConsumerType.RemainingCategory, RegionId =  6 },
                new ConsumerBinding{ Id = 13, CapacityConsumerType =     CapacityConsumerType.FirstCategory, RegionId =  7 },
                new ConsumerBinding{ Id = 14, CapacityConsumerType = CapacityConsumerType.RemainingCategory, RegionId =  7 },
                new ConsumerBinding{ Id = 15, CapacityConsumerType =     CapacityConsumerType.FirstCategory, RegionId =  8 },
                new ConsumerBinding{ Id = 16, CapacityConsumerType = CapacityConsumerType.RemainingCategory, RegionId =  8 },
                new ConsumerBinding{ Id = 17, CapacityConsumerType =     CapacityConsumerType.FirstCategory, RegionId =  9 },
                new ConsumerBinding{ Id = 18, CapacityConsumerType = CapacityConsumerType.RemainingCategory, RegionId =  9 },
                new ConsumerBinding{ Id = 19, CapacityConsumerType =     CapacityConsumerType.FirstCategory, RegionId = 10 },
                new ConsumerBinding{ Id = 20, CapacityConsumerType = CapacityConsumerType.RemainingCategory, RegionId = 10 },

            };

            
            modelBuilder.Entity<PowerSource>().HasData(powerSources);
            modelBuilder.Entity<Consumer>().HasData(consumers);
            modelBuilder.Entity<ConnectedRegion>().HasData(connectedRegions);
            modelBuilder.Entity<Region>().HasData(regions);
            modelBuilder.Entity<ProducerBinding>().HasData(producerBindings);
            modelBuilder.Entity<ConsumerBinding>().HasData(consumerBindings);
        }
    }
}
