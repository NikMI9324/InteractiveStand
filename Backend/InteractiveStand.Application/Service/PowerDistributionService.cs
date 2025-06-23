using InteractiveStand.Application.Hubs;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Application.RegionMetricsClass;
using InteractiveStand.Application.State;
using InteractiveStand.Domain.Classes;
using InteractiveStand.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;


namespace InteractiveStand.Application.Service
{
    
    public class PowerDistributionService : IPowerDistributionService, IDisposable
    {
        private enum SimulationState { Stopped, Running, Paused };
        private SimulationState _currentState = SimulationState.Stopped;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<EnergyDistributionHub> _hubContext;
        private readonly ILogger<PowerDistributionService> _logger;
        private readonly int _baseTimeZoneOffset = 3;

        private const double DayInSeconds = 24 * 3600;
        private double _speedFactor = 1.0;
        private int _totalTicks;
        private int _currentTick = 0;
        private PeriodicTimer _simulationTimer;
        private CancellationTokenSource _simulationCts;
        private readonly SemaphoreSlim _simulationLock = new(1);

        public PowerDistributionService(IServiceScopeFactory scopeFactory,IHubContext<EnergyDistributionHub> hubContext,
        ILogger<PowerDistributionService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }
        public void Dispose()
        {
            _simulationTimer?.Dispose();
            _simulationCts?.Dispose();
            _simulationLock.Dispose();
        }
        public async Task StartSimulationAsync(int speedFactor)
        {
            await _simulationLock.WaitAsync();
            try 
            {
                if (_currentState != SimulationState.Stopped)
                    throw new InvalidOperationException("Simulation is already running.");
                _speedFactor = speedFactor > 0 ? speedFactor : 1.0;
                _totalTicks = Math.Max(1, (int)Math.Ceiling(DayInSeconds / _speedFactor));
                _currentTick = 0;
                _simulationCts = new CancellationTokenSource();

                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<RegionDbContext>();
                    var connectedRegions = await context.ConnectedRegions.ToListAsync();
                    foreach (var cr in connectedRegions)
                    {
                        cr.ReceivedFirstCategoryCapacity = 0;
                        cr.SentFirstCategoryCapacity = 0;
                        cr.ReceivedRemainingCapacity = 0;
                        cr.SentRemainingCapacity = 0;
                    }
                    await context.SaveChangesAsync();
                }
                _simulationTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                _currentState = SimulationState.Running;
                _ = RunSimulationAsync(_simulationCts.Token);
                await _hubContext.Clients.All.SendAsync("ReceiveSimulationUpdate",
                $"Simulation started at 00:00:00 (GMT{_baseTimeZoneOffset:+#;-#;0}) with {(_speedFactor / 3600):F2} hours per second.");
            }
            finally
            {
                _simulationLock.Release();
            }
            
        }
        private async Task RunSimulationAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (await _simulationTimer.WaitForNextTickAsync(cancellationToken))
                {
                    await _simulationLock.WaitAsync(cancellationToken);
                    try
                    {
                        if (_currentTick >= _totalTicks)
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveSimulationUpdate",
                                "Simulation was successfully completed");
                            await _hubContext.Clients.All.SendAsync("SimulationFinished");
                            await StopSimulationAsync();
                            return;
                        }

                        using var scope = _scopeFactory.CreateScope();

                        var context = scope.ServiceProvider.GetRequiredService<RegionDbContext>();
                        await UpdateSimulation(context, cancellationToken);
                        _currentTick++;
                    }
                    finally
                    {
                        _simulationLock.Release();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Simulation was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in simulation");
                await _hubContext.Clients.All.SendAsync("ReceiveSimulationUpdate", $"Simulation error: {ex.Message}");
                
            }
            finally
            {
                await CleanupSimulation();
            }
        }
        private async Task UpdateSimulation(RegionDbContext context, CancellationToken cancellationToken)
        {
            var regions = await context.Regions
                                        .Include(r => r.PowerSource)
                                        .Include(r => r.Consumer)
                                        .ToListAsync();
            double globalSimulationTimeSeconds = _currentTick * _speedFactor;
            double globalSimulationTimeHours = globalSimulationTimeSeconds / 3600;
            double baseHourFraction = globalSimulationTimeHours % 24;

            foreach (var region in regions)
            {
                var regionalHourFraction = CalculateRegionalHourFraction(region, baseHourFraction);
                var stepSeconds = _speedFactor;

                var regionMetrics = await CalculateRegionMetrics(
                    context, 
                    region, 
                    regionalHourFraction, 
                    stepSeconds, 
                    cancellationToken);

                var status = BuildRegionStatus(region, globalSimulationTimeSeconds, regionMetrics);
                await SendSimulationUpdate(status, cancellationToken);

                if (regionMetrics.FirstCategoryDeficit > 0 || regionMetrics.RemainingDeficit > 0)
                {
                    await HandleRegionDeficit(
                        context, 
                        region, 
                        regionalHourFraction, 
                        regionMetrics, 
                        cancellationToken);
                }
            }
        }
        private double CalculateRegionalHourFraction(Region region, double baseHourFraction)
        {
            double fraction = (baseHourFraction + (region.TimeZoneOffset - _baseTimeZoneOffset)) % 24;
            return fraction < 0 ? fraction + 24 : fraction;
        }
        private async Task HandleRegionDeficit(
            RegionDbContext context,
            Region region,
            double hourFraction,
            RegionMetrics metrics,
            CancellationToken cancellationToken)
        {
            await DistributePowerRecursivelyAsync(
                region.Id,
                new HashSet<int>(),
                hourFraction,
                context,
                cancellationToken);

            var connections = await context.ConnectedRegions.ToListAsync(cancellationToken);
            await SendConnectionUpdate(connections, region, metrics.SimulationTimeSeconds, cancellationToken);
        }

        private Task<RegionMetrics> CalculateRegionMetrics(
            RegionDbContext context,
            Region region,
            double hourFraction,
            double stepSeconds,
            CancellationToken cancellationToken)
        {
            double consumed = IntegrateConsumption(region, hourFraction, stepSeconds);
            double produced = IntegrateProduction(region, hourFraction, stepSeconds);

            double firstCategoryConsumed = consumed * (region.Consumer?.FirstPercentage ?? 0) / 100;
            double firstCategoryProduced = region.PowerSource?.CalculateAvailableCapacityForFirstCategory(region.ProducedCapacity) ?? 0;

            return Task.FromResult(new RegionMetrics
            {
                ConsumedEnergy = consumed,
                ProducedEnergy = produced,
                FirstCategoryDeficit = firstCategoryConsumed - firstCategoryProduced,
                RemainingDeficit = (consumed - firstCategoryConsumed) -
                                  (produced - Math.Min(firstCategoryProduced, firstCategoryConsumed))
            });
        }
        private async Task DistributePowerRecursivelyAsync(
            int regionId,
            HashSet<int> visited,
            double hourFraction,
            RegionDbContext context,
            CancellationToken cancellationToken)
        {
            if (visited.Contains(regionId))
            {
                return;
            }
            visited.Add(regionId);

            var region = await GetRegionWithDependenciesAsync(context, regionId, cancellationToken);

            if (region == null || region.Consumer == null || region.PowerSource == null)
                return;
            double stepSeconds = _speedFactor;
            var regionMetrics = await CalculateRegionMetrics(
                context,
                region,
                hourFraction,
                stepSeconds,
               cancellationToken);
            if (regionMetrics.FirstCategoryDeficit <= 0 && regionMetrics.RemainingDeficit <= 0) return;

            var connections = await context.ConnectedRegions
                                           .Where(cr => cr.RegionSourceId == regionId)
                                           .ToListAsync(cancellationToken);
            foreach (var connection in connections)
            {
                if (regionMetrics.FirstCategoryDeficit <= 0 && regionMetrics.RemainingDeficit <= 0)
                    break;
                var neighborRegion = await GetRegionWithDependenciesAsync(
                    context,
                    connection.RegionDestinationId,
                    cancellationToken);
                if (neighborRegion == null || neighborRegion.Consumer == null || neighborRegion.PowerSource == null)
                    continue;
                double neighbourHourFraction = CalculateNeighbourHourFraction(region, neighborRegion, hourFraction);
                var neighbourMetrics = await CalculateRegionMetrics(
                    context,
                    neighborRegion,
                    neighbourHourFraction,
                    stepSeconds,
                    cancellationToken);
                if (regionMetrics.FirstCategoryDeficit > 0 && neighbourMetrics.FirstCategoryDeficit < 0)
                {
                    double transferAmount = Math.Min(
                        regionMetrics.FirstCategoryDeficit,
                        -neighbourMetrics.FirstCategoryDeficit);

                    await ExecuteTransfer(
                        context,
                        region.Id,
                        neighborRegion.Id,
                        transferAmount,
                        isFirstCategory: true,
                        cancellationToken);

                    regionMetrics.FirstCategoryDeficit -= transferAmount;
                }

                if (regionMetrics.RemainingDeficit > 0 && neighbourMetrics.RemainingDeficit < 0)
                {
                    double transferAmount = Math.Min(
                        regionMetrics.RemainingDeficit,
                        -neighbourMetrics.RemainingDeficit);

                    await ExecuteTransfer(
                        context,
                        region.Id,
                        neighborRegion.Id,
                        transferAmount,
                        isFirstCategory: false,
                        cancellationToken);

                    regionMetrics.RemainingDeficit -= transferAmount;
                }
                await DistributePowerRecursivelyAsync(
                    neighborRegion.Id,
                    visited,
                    neighbourHourFraction,
                    context,
                    cancellationToken);
            }
        }
        private async Task ExecuteTransfer(
            RegionDbContext context,
            int recipientId,
            int donorId,
            double amount,
            bool isFirstCategory,
            CancellationToken cancellationToken)
        {
            var directConnection = await context.ConnectedRegions
                .FirstOrDefaultAsync(cr =>
                    cr.RegionSourceId == donorId &&
                    cr.RegionDestinationId == recipientId,
                    cancellationToken);

            var reverseConnection = await context.ConnectedRegions
                .FirstOrDefaultAsync(cr =>
                    cr.RegionSourceId == recipientId &&
                    cr.RegionDestinationId == donorId,
                    cancellationToken);

            if (directConnection != null && reverseConnection != null)
            {
                if (isFirstCategory)
                {
                    directConnection.ReceivedFirstCategoryCapacity += amount;
                    reverseConnection.SentFirstCategoryCapacity += amount;
                }
                else
                {
                    directConnection.ReceivedRemainingCapacity += amount;
                    reverseConnection.SentRemainingCapacity += amount;
                }

                await context.SaveChangesAsync(cancellationToken);
                await NotifyTransfer(recipientId, donorId, amount, isFirstCategory);
            }
        }
        private async Task NotifyTransfer(
            int recipientId,
            int donorId,
            double amount,
            bool isFirstCategory)
        {
            string category = isFirstCategory ? "First" : "Remaining";
            await _hubContext.Clients.All.SendAsync(
                "ReceiveDistributionUpdate",
                $"Transferred {amount:F3} ({category} category) from {donorId} to {recipientId}");
        }
        private double CalculateNeighbourHourFraction(Region current, Region neighbor, double currentHourFraction)
        {
            int timeZoneDiff = neighbor.TimeZoneOffset - current.TimeZoneOffset;
            return (currentHourFraction + timeZoneDiff) % 24;
        }
        private async Task<Region?> GetRegionWithDependenciesAsync(
            RegionDbContext context,
            int regionId,
            CancellationToken cancellationToken)
        {
            return await context.Regions
                .Include(r => r.Consumer)
                .Include(r => r.PowerSource)
                .FirstOrDefaultAsync(r => r.Id == regionId, cancellationToken);
        }
        private string BuildRegionStatus(Region region, double simulationTimeSeconds, RegionMetrics metrics)
        {
            return $"Region {region.Id} : {region.Name} at {FormatTime(simulationTimeSeconds)} " +
                   $"(GMT{region.TimeZoneOffset:+#;-#;0}) : Consumed = {metrics.ConsumedEnergy:F2}, " +
                   $"Produced = {metrics.ProducedEnergy:F2}, First category deficit = {Math.Min(metrics.FirstCategoryDeficit,0):F4}, " +
                   $"Remaining category deficit = {Math.Min(metrics.FirstCategoryDeficit, 0):F4}";
        }
        private async Task SendSimulationUpdate(string message, CancellationToken cancellationToken)
        {
            PowerDistributionState.SimulationLogs.Add(message);
            await _hubContext.Clients.All.SendAsync(
                "ReceiveSimulationUpdate",
                message,
                cancellationToken);
        }
        private async Task SendConnectionUpdate(
            List<ConnectedRegion> connections,
            Region region,
            double simulationTimeSeconds,
            CancellationToken cancellationToken)
        {
            string update = $"Connections updated at {FormatTime(simulationTimeSeconds)} " +
                           $"(GMT{region.TimeZoneOffset:+#;-#;0}): {JsonSerializer.Serialize(connections)}";

            PowerDistributionState.SimulationLogs.Add(update);
            await _hubContext.Clients.All.SendAsync(
                "ReceiveConnectionUpdate",
                connections,
                cancellationToken);
        }
        private async Task CleanupSimulation()
        {
            _simulationTimer?.Dispose();
            _simulationTimer = null;
            _simulationCts?.Dispose();
            _simulationCts = null;
            _currentTick = 0;
        }
        private double IntegrateConsumption(Region region, double startHourFraction, double stepSeconds)
        {
            double start = startHourFraction;
            double end = (startHourFraction + (stepSeconds / 3600)) % 24;
            if (end < start) end += 24; 
            int steps = 10; 
            double h = (end - start) / steps;
            double sum = 0.0;

            for (int i = 0; i <= steps; i++)
            {
                double x = start + i * h;
                double y = region.CalculateHourlyConsumption(x);
                sum += (i == 0 || i == steps) ? y / 2 : y;
            }

            return sum * h;
        }
        private double IntegrateProduction(Region region, double startHourFraction, double stepSeconds)
        {
            double intervalHours = stepSeconds / 3600; 
            return region.HourlyProducedCapacity * intervalHours;
        }
        public async Task StopSimulationAsync()
        {
            await _simulationLock.WaitAsync();
            try
            {
                if (_currentState == SimulationState.Stopped)
                    return;

                _simulationCts?.Cancel();
                await CleanupSimulation();
                _currentState = SimulationState.Stopped;

                await _hubContext.Clients.All.SendAsync("ReceiveSimulationUpdate", "Simulation stopped.");
            }
            finally
            {
                _simulationLock.Release();
            }
        }
        public async Task PauseSimulationAsync()
        {
            await _simulationLock.WaitAsync();
            try
            {
                if (_currentState != SimulationState.Running)
                    throw new InvalidOperationException("Simulation is not running.");

                _simulationCts.Cancel();
                _currentState = SimulationState.Paused;

                await _hubContext.Clients.All.SendAsync(
                    "ReceiveSimulationUpdate",
                    $"Simulation paused at {FormatTime(_currentTick * _speedFactor)}.");
            }
            finally
            {
                _simulationLock.Release();
            }
        }

        public async Task ResumeSimulationAsync()
        {
            await _simulationLock.WaitAsync();
            try
            {
                if (_currentState != SimulationState.Paused)
                    throw new InvalidOperationException("Simulation is not paused.");

                _simulationCts = new CancellationTokenSource();
                _simulationTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                _currentState = SimulationState.Running;

                _ = RunSimulationAsync(_simulationCts.Token);

                await _hubContext.Clients.All.SendAsync(
                    "ReceiveSimulationUpdate",
                    $"Simulation resumed at {FormatTime(_currentTick * _speedFactor)}.");
            }
            finally
            {
                _simulationLock.Release();
            }
        }
        private string FormatTime(double seconds)
        {
            int hours = (int)(seconds / 3600) % 24;
            int minutes = (int)((seconds % 3600) / 60);
            int secs = (int)(seconds % 60);
            return $"{hours:00}:{minutes:00}:{secs:00}";
        }
        public List<string> GetSimulationLogs()
        {
            return PowerDistributionState.SimulationLogs.ToList();
        }
    }
}
