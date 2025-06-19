using InteractiveStand.Application.Hubs;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Domain.Classes;
using InteractiveStand.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Text.Json;

namespace InteractiveStand.Application.Service
{
    public class PowerDistributionState
    {
        public static bool IsSimulationRunning { get; set; }
        public static CancellationTokenSource CancellationTokenSource { get; set; }
        public static List<string> SimulationLogs { get; } = new List<string>();
    }
    public class PowerDistributionService : IPowerDistributionService
    {
        private readonly RegionDbContext _context;
        private readonly IHubContext<EnergyDistributionHub> _hubContext;
        private readonly List<string> _simulationLogs = new List<string>();
        private static readonly object _lockObject = new object();
        public PowerDistributionService(RegionDbContext context, IHubContext<EnergyDistributionHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext;
        }

        private double GetSimulationStepSeconds(int speedFactor)
        {
            return speedFactor switch
            {
                1 => 5.0,    
                6 => 30.0,  
                60 => 300.0, 
                360 => 1800.0,
                600 => 3000.0, 
                720 => 3600.0,
                _ => throw new ArgumentException("Invalid speed factor. Use 1, 6, 60, 360, 600, or 720.")
            };
        }
        public async Task StartSimulationAsync(int speedFactor, CancellationToken cancellationToken)
        {
            lock (_lockObject)
            {
                if (PowerDistributionState.IsSimulationRunning)
                    throw new InvalidOperationException("Simulation is already running.");
                PowerDistributionState.IsSimulationRunning = true;
                PowerDistributionState.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            }

            var regions = await _context.Regions
                                        .Include(r => r.PowerSource)
                                        .Include(r => r.Consumer)
                                        .ToListAsync();
            double secondsPerRealSecond = GetSimulationStepSeconds(speedFactor);
            TimeSpan simulationStep = TimeSpan.FromSeconds(secondsPerRealSecond);

            await _hubContext.Clients.All.SendAsync("ReceiveSimulationUpdate", "Simulation started.");

            try
            {
                for (double seconds = 0; seconds < 3600 * 24; seconds += simulationStep.TotalSeconds)
                {
                    if (PowerDistributionState.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveSimulationUpdate", "Simulation stopped.");
                        break;
                    }

                    double hourFraction = (seconds / 3600) % 24;
                    foreach (var region in regions)
                    {
                        double localHourFraction = (seconds / 3600) % 24;
                        region.HourFraction = localHourFraction;

                        string status = $"Region {region.Id} : {region.Name} at {region.HourFraction:F2}h : " +
                            $"Consumption = {region.CurrentCapacityConsumption:F2}, Produced = {region.HourlyProducedCapacity:F2}" +
                            $", First Category Deficit = {region.FirstCategoryDeficit:F2}, Remaining Deficit = {region.RemainingDeficit:F2}";
                        PowerDistributionState.SimulationLogs.Add(status);
                        await _hubContext.Clients.All.SendAsync("ReceiveSimulationUpdate", status);

                        if (region.FirstCategoryDeficit > 0 || region.RemainingDeficit > 0)
                        {
                            await DistributePowerRecursivelyAsync(region.Id, new HashSet<int>());
                            var connections = await _context.ConnectedRegions.ToListAsync();
                            string connectionUpdate = $"Connections updated at {hourFraction:F2}h: {JsonSerializer.Serialize(connections)}";
                            PowerDistributionState.SimulationLogs.Add(connectionUpdate);
                            await _hubContext.Clients.All.SendAsync("ReceiveConnectionUpdate", connections);
                        }
                    }
                    await Task.Delay(1000, PowerDistributionState.CancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveSimulationUpdate", "Simulation stopped due to cancellation.");
            }
            catch (Exception ex)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveSimulationUpdate", $"Error during simulation: {ex.Message}");
                throw;
            }
            finally
            {
                lock (_lockObject)
                {
                    PowerDistributionState.IsSimulationRunning = false;
                    PowerDistributionState.CancellationTokenSource?.Dispose();
                    PowerDistributionState.CancellationTokenSource = null;
                }
            }

            await _hubContext.Clients.All.SendAsync("ReceiveSimulationUpdate", "Simulation completed.");
        }
        public async Task StopSimulationAsync()
        {
            lock (_lockObject)
            {
                if (!PowerDistributionState.IsSimulationRunning)
                    throw new InvalidOperationException("Simulation is not running.");
                PowerDistributionState.CancellationTokenSource?.Cancel();
            }
            await Task.CompletedTask;
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

            if (firstCategoryDeficit <= 0 && remainingDeficit <= 0) return;

            var connectedToSourceRegion = await _context.ConnectedRegions
                .Where(cr => cr.RegionSourceId == regionId)
                .ToListAsync();

            foreach (var connectedRegion in connectedToSourceRegion)
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

                        connectedRegion.ReceivedFirstCategoryCapacity += transferAmount;
                        var reverseConnection = await _context.ConnectedRegions
                            .FirstOrDefaultAsync(cr => cr.RegionSourceId == neighbourRegionId && cr.RegionDestinationId == regionId);
                        reverseConnection.SentFirstCategoryCapacity += transferAmount;
                        await _context.SaveChangesAsync();

                        await _hubContext.Clients.All.SendAsync("ReceiveDistributionUpdate",
                            $"Transferred {transferAmount:F2} (First) from {neighbourRegionId} : {neighbourRegion.Name} to {regionId} : {region.Name}");
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
                        connectedRegion.ReceivedRemainingCapacity += transferAmount;
                        var reverseConnection = await _context.ConnectedRegions
                            .FirstOrDefaultAsync(cr => cr.RegionSourceId == neighbourRegionId && cr.RegionDestinationId == regionId);
                        reverseConnection.SentRemainingCapacity += transferAmount;
                        await _context.SaveChangesAsync();

                        await _hubContext.Clients.All.SendAsync("ReceiveDistributionUpdate",
                            $"Transferred {transferAmount:F2} (First) from {neighbourRegionId} : {neighbourRegion.Name} to {regionId} : {region.Name}");
                    }
                }

                await DistributePowerRecursivelyAsync(neighbourRegionId, visited);

            }
        }

        public List<string> GetSimulationLogs()
        {
            return PowerDistributionState.SimulationLogs.ToList();
        }
    }
}
