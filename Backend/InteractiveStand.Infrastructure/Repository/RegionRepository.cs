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

        public async Task<Region> GetFullInfoRegionByIdAsync(int regionId)
        {
            var region = await _context.Regions
                                       .Include(r => r.Consumer)
                                       .Include(r => r.PowerSource)
                                       .FirstOrDefaultAsync(r => r.Id == regionId);
            if(region is null)
                throw new KeyNotFoundException($"Region with ID {regionId} not found.");
            return region;
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

        public async Task<Region> GetRegionByIdAsync(int id)
        {
            var region = await _context.Regions
                                       .Include(r => r.Consumer)
                                       .Include(r => r.PowerSource)
                                       .FirstOrDefaultAsync(r => r.Id == id);
            if (region is null)
            {
                throw new KeyNotFoundException($"Region with ID {id} not found.");
            }
            if(region.Consumer is null || region.PowerSource is null)
            {
                throw new InvalidOperationException($"Region {id} must have both a consumer and a power source.");
            }
            return region;
        }

        public async Task ResetDataAsync()
        {
            _context.ConnectedRegions.RemoveRange(_context.ConnectedRegions);
            _context.Regions.RemoveRange(_context.Regions);
            _context.PowerSources.RemoveRange(_context.PowerSources);
            _context.Consumers.RemoveRange(_context.Consumers);

            await _context.SaveChangesAsync();
            await _context.PowerSources.AddRangeAsync(InitialData.PowerSources);
            await _context.Consumers.AddRangeAsync(InitialData.Consumers);
            await _context.Regions.AddRangeAsync(InitialData.Regions);
            await _context.ConnectedRegions.AddRangeAsync(InitialData.ConnectedRegions);
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

        public async Task UpdateRegion(Region region)
        {
            _context.Regions.Update(region);
            await _context.SaveChangesAsync();
        }
    }
}
