using InteractiveStand.Application.Interfaces;
using InteractiveStand.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStand.Application.Service
{
    public class PowerDistributionService : IPowerDistributionService
    {
        private readonly RegionDbContext _context;

        public PowerDistributionService(RegionDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task DistributePowerAsync(int regionId)
        {
            var region = _context.Regions
                                 .Include(r => r.Consumer)
                                 .Include(r => r.PowerSource)
                                 .FirstOrDefault(r => r.Id == regionId);
            if (region is null)
            {
                throw new KeyNotFoundException($"Region with ID {regionId} not found.");
            }

            var (isEnoughForFirstCategory, isEnoughRemainingCapacity) = region.GetCapacityStatus();
            if(isEnoughForFirstCategory && isEnoughRemainingCapacity)
            {
                return;
            }
            var visited = new HashSet<int>();
            await DistributePowerRecursivelyAsync(regionId, visited);
        }
        private async Task DistributePowerRecursivelyAsync(int regionId, HashSet<int> visited)
        {
            if(visited.Contains(regionId))
            {
                return;
            }
            visited.Add(regionId);
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
            var (isEnoughForFirstCategory, isEnoughRemainingCapacity) = region.GetCapacityStatus();
            if (isEnoughForFirstCategory && isEnoughRemainingCapacity)
            {
                return; 
            }
            double firstCategoryDeficit = region.FirstCategoryDeficit;
            double remainingDeficit = region.RemainingDeficit;

            if (firstCategoryDeficit <= 0 && remainingDeficit <= 0)
            {
                return;
            }

            var connectedRegions = await _context.ConnectedRegions
                .Where(cr => cr.RegionSourceId == regionId)
                .ToListAsync();

            

            foreach (var connectedRegion in connectedRegions)
            {
                if (firstCategoryDeficit <= 0 && remainingDeficit <= 0)
                    break;
                var neighbourRegionId = connectedRegion.RegionDestinationId;
                var neighbourRegion = await _context.Regions
                    .Include(r => r.Consumer)
                    .Include(r => r.PowerSource)
                    .FirstOrDefaultAsync(r => r.Id == neighbourRegionId);

                if (neighbourRegion is null || neighbourRegion.PowerSource is null || neighbourRegion.Consumer is null)
                    continue;

                var (neighbourIsEnoughForFirstCategory, neighbourIsEnoughRemainingCapacity) = neighbourRegion.GetCapacityStatus();
                if (!neighbourIsEnoughForFirstCategory || !neighbourIsEnoughRemainingCapacity)
                    continue;
                
                if(firstCategoryDeficit > 0)
                {
                    double neighbourFirstCategorySurplus = neighbourRegion.FirstCategoryProducedCapacity -
                        neighbourRegion.FirstCategoryConsumedCapacity;
                    if (neighbourFirstCategorySurplus > 0)
                    {
                        double transferAmount = Math.Min(firstCategoryDeficit, neighbourFirstCategorySurplus);
                        firstCategoryDeficit -= transferAmount;

                        connectedRegion.SentFirstCategoryCapacity += transferAmount;
                        var reverseConnection = await _context.ConnectedRegions
                            .FirstOrDefaultAsync(cr => cr.RegionSourceId == neighbourRegionId && cr.RegionDestinationId == regionId);
                        reverseConnection.ReceivedFirstCategoryCapacity += transferAmount;
                        await _context.SaveChangesAsync();

                    }

                    
                }
                if(remainingDeficit > 0)
                {
                    double neighbourRemainingSurplus = neighbourRegion.RemainingProducedCapacity -
                        neighbourRegion.RemainingConsumedCapacity;
                    if (neighbourRemainingSurplus > 0)
                    {
                        double transferAmount = Math.Min(remainingDeficit, neighbourRemainingSurplus);
                        remainingDeficit -= transferAmount;
                        connectedRegion.SentRemainingCapacity += transferAmount;
                        var reverseConnection = await _context.ConnectedRegions
                            .FirstOrDefaultAsync(cr => cr.RegionSourceId == neighbourRegionId && cr.RegionDestinationId == regionId);
                        reverseConnection.ReceivedRemainingCapacity += transferAmount;
                        await _context.SaveChangesAsync();
                    }
                }

                await DistributePowerRecursivelyAsync(neighbourRegionId, visited);

            }
        }
    }
}
