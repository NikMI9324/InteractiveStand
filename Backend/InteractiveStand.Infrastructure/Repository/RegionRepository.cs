using InteractiveStand.Domain.Classes;
using InteractiveStand.Domain.Interfaces;
using InteractiveStand.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace InteractiveStand.Infrastructure.Repository
{
    public class RegionRepository : IRegionRepository
    {
        private readonly RegionDbContext _context;
        public RegionRepository(RegionDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Region>> GetAllRegionsAsync()
        {
            return await _context.Regions
                                 .Include(r => r.Consumer)
                                 .Include(r => r.PowerSource)
                                 .ToListAsync();
        }

        public async Task<Consumer> GetComsumerByIdAsync(int id)
        {
            var consumer = await _context.Consumers.FirstOrDefaultAsync(c => c.Id == id);
            if (consumer is null)
            {
                throw new KeyNotFoundException($"Consumer with ID {id} not found.");
            }
            return consumer;
        }

        public async Task<PowerSource> GetPowerSourceByIdAsync(int id)
        {
            var powerSource = await _context.PowerSources.FirstOrDefaultAsync(ps => ps.Id == id);
            if (powerSource is null)
            {
                throw new KeyNotFoundException($"Power source with ID {id} not found.");
            }
            return powerSource;
        }

        public async Task<Region> GetRegionByIdAsync(int regionId)
        {
            var region = await _context.Regions
                                       .Include(r => r.Consumer)
                                       .Include(r => r.PowerSource)
                                       .FirstOrDefaultAsync(r => r.Id == regionId);
            if (region is null)
            {
                throw new KeyNotFoundException($"Region with ID {regionId} not found.");
            }
            if(region.Consumer is null || region.PowerSource is null)
            {
                throw new InvalidOperationException($"Region {regionId} must have both a consumer and a power source.");
            }
            return region;
        }
        private async Task UpsertRangeAsync<T>(DbSet<T> dbSet, IEnumerable<T> newData) where T: class
        {
            foreach(var entity in newData)
            {
                var id = _context.Entry(entity).Property("Id").CurrentValue;
                var existing = await dbSet.FindAsync(id);
                if (existing != null)
                    _context.Entry(existing).CurrentValues.SetValues(entity);
                else
                    await dbSet.AddAsync(entity);
            }
        }


        public async Task ResetDataAsync()
        {
            await UpsertRangeAsync(_context.PowerSources, InitialData.PowerSources);
            await UpsertRangeAsync(_context.Consumers, InitialData.Consumers);
            await UpsertRangeAsync(_context.Regions, InitialData.Regions);
            await UpsertRangeAsync(_context.ConnectedRegions, InitialData.ConnectedRegions);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateConsumerAsync(Consumer consumer)
        {
            _context.Consumers.Update(consumer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePowerSourceAsync(PowerSource powerSource)
        {
            _context.PowerSources.Update(powerSource);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRegionAsync(Region region)
        {
            _context.Regions.Update(region);
            await _context.SaveChangesAsync();
        }

        public async Task ResetConnectedRegionCapacityValuesAsync()
        {
            var connectedRegions = _context.ConnectedRegions.ToListAsync();
            foreach(var cr in connectedRegions.Result)
            {
                cr.ReceivedFirstCategoryCapacity = 0;
                cr.SentFirstCategoryCapacity = 0;
                cr.ReceivedRemainingCapacity = 0;
                cr.SentRemainingCapacity = 0;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProducerBinding>> GetProducerBindingsWithRegionAsync(CancellationToken token)
        {
            return await _context.ProducerBindings
                    .Include(pb => pb.Region)
                        .ThenInclude(r => r.PowerSource)
                    .ToListAsync(token);
        }

        public async Task<List<ConsumerBinding>> GetConsumerBindingsWithRegionAsync( CancellationToken token)
        {
            return await _context.ConsumerBindings
                .Include(cb => cb.Region)
                .ToListAsync(token);
        }

        public async Task ResetRegionStatusAsync()
        {
            var regions = await _context.Regions.ToListAsync();
            foreach (var region in regions)
                region.IsActive = true;
            await _context.SaveChangesAsync();
        }
    }
}
