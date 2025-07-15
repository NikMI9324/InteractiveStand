using AutoMapper;
using InteractiveStand.Application.Dtos.RegionDto;
using InteractiveStand.Application.Hubs;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Application.RegionMetricsClasses;
using InteractiveStand.Domain.Classes;
using InteractiveStand.Domain.Interfaces;
using InteractiveStand.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Data;


namespace InteractiveStand.Application.Services
{
    
    public class PowerDistributionService : IPowerDistributionService, IDisposable
    {
        private enum SimulationState { Stopped, Running, Paused };
        private SimulationState _currentState = SimulationState.Stopped;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<EnergyDistributionHub> _hubContext;
        private readonly int _baseTimeZoneOffset = 3;
        private readonly IMqttSimulationPublisher _mqttSimulationPublisherService;
        private readonly IMapper _mapper;

        private const double DayInSeconds = 24 * 3600;
        private double _speedFactor = 1.0;
        private int _totalTicks;
        private int _currentTick = 0;
        private PeriodicTimer _simulationTimer;
        private CancellationTokenSource _simulationCts;
        private readonly SemaphoreSlim _simulationLock = new(1);

        public PowerDistributionService(
            IServiceScopeFactory scopeFactory,
            IHubContext<EnergyDistributionHub> hubContext,
            IMqttSimulationPublisher mqttSimulationPublisherService,
            IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _mqttSimulationPublisherService = mqttSimulationPublisherService;
            _mapper = mapper;
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
                    var repo = scope.ServiceProvider.GetRequiredService<IRegionRepository>();
                    await repo.ResetConnectedRegionCapacityValuesAsync();
                    await repo.ResetRegionStatusAsync();
                }
                _simulationTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                _currentState = SimulationState.Running;
                _ = RunSimulationAsync(_simulationCts.Token);

                await _hubContext.Clients.All.SendAsync("ReceiveSimulationStatus",
                new
                {
                    Status = "Started",
                    Message = $"Simulation started (GMT{_baseTimeZoneOffset:+#;0})" +
                    $"with {(_speedFactor / 3600):F2} hours per second."
                });
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
                            await _hubContext.Clients.All.SendAsync("ReceiveSimulationStatus",
                            new { Status = "Completed", Message = "Simulation was successfully completed" });
                            await _hubContext.Clients.All.SendAsync("SimulationFinished");
                            _simulationLock.Release();
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
                await _hubContext.Clients.All.SendAsync("ReceiveSimulationStatus",
                    new { Status = "Canceled", Message = "Simulation was canceled" });
            }
            catch (Exception ex)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveSimulationStatus",
                    new { Status = "Error", Message = $"Simulation error: {ex.Message}" });
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
            await _hubContext.Clients.All.SendAsync("ReceiveCurrentTime", new { 
                Time = FormatTime(globalSimulationTimeSeconds) 
            });

            var metricsCache = new Dictionary<int, RegionMetrics>();
            var regionMetricsList = new List<RegionMetricsDto>();
            var metricsList = new List<(Region region, RegionMetrics regionMetrcis)>();
            
            foreach (var region in regions)
            {
                var regionalHourFraction = CalculateRegionalHourFraction(region, baseHourFraction);
                var stepSeconds = _speedFactor;

                var regionMetrics = await CalculateRegionMetrics(
                    region,
                    regionalHourFraction,
                    stepSeconds);
                metricsCache[region.Id] = regionMetrics;
                metricsList.Add((region, regionMetrics));
            }
            metricsList.Sort(Comparer<(Region region, RegionMetrics regionMetrics)>.Create((el1, el2) =>
            {
                var rm1 = el1.regionMetrics;
                var rm2 = el2.regionMetrics;
                var cmp = rm1.FirstCategoryDeficit.CompareTo(rm2.FirstCategoryDeficit);
                if (cmp != 0)
                    return cmp;
                return rm1.RemainingDeficit.CompareTo(rm2.RemainingDeficit);
            }));
            foreach (var (region, metric) in metricsList)
            {
                double regionalHourFraction = CalculateRegionalHourFraction(region, baseHourFraction);
                if (metric.FirstCategoryDeficit > 0)
                {
                    await HandleRegionDeficit(
                        context,
                        region,
                        regionalHourFraction,
                        metricsList,
                        true,
                        cancellationToken);
                }
                if (metric.RemainingDeficit > 0)
                {
                    await HandleRegionDeficit(
                        context,
                        region,
                        regionalHourFraction,
                        metricsList,
                        false,
                        cancellationToken);
                }
            }
            _ = Task.Run(async () =>
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(2)); 
                try
                {
                    await PublishConsumerMessage(baseHourFraction, timeoutCts.Token);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("[MQTT] PublishConsumerMessage прерван из-за таймаута");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MQTT] Ошибка при публикации: {ex.Message}");
                }
            });
            var simulationDtoList = regions.Select(region =>
            {
                var metrics = metricsList.FirstOrDefault(x => x.region == region).regionMetrcis;
                metrics.SimulationTimeSeconds = globalSimulationTimeSeconds;

                return _mapper.Map<RegionSimulationDto>((region, metrics));
            }).ToList();

            await _hubContext.Clients.All.SendAsync("ReceiveRegionMetrics", simulationDtoList);
        }
        private async Task PublishConsumerMessage(double baseHourFraction,CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var regionRepo = scope.ServiceProvider.GetRequiredService<IRegionRepository>();
            var consumerBindings = await regionRepo.GetConsumerBindingsWithRegionAsync(cancellationToken);
            foreach(var consumerBinding in consumerBindings)
            {
                double regionHourFraction = CalculateRegionalHourFraction(consumerBinding.Region, baseHourFraction);
                await _mqttSimulationPublisherService.PublishRegionConsumerStatusAsync(consumerBinding, regionHourFraction, cancellationToken);
            }
        }
        private async Task HandleRegionDeficit(
            RegionDbContext context,
            Region region,
            double hourFraction,
            List<(Region region, RegionMetrics regionMetrics)> metricsList,
            bool isFirstCategoryDistributed,
            CancellationToken cancellationToken)
        {
            var visited = new HashSet<int>();
            double stepSeconds = _speedFactor;

            bool resolved = await DistributePowerRecursivelyAsync(
                context,
                region,
                visited,
                hourFraction,
                metricsList,
                isFirstCategoryDistributed,
                cancellationToken);

            region.IsActive = resolved;
            await context.SaveChangesAsync(cancellationToken);

            var connections = await context.ConnectedRegions.ToListAsync(cancellationToken);
            await SendConnectionUpdate(
                connections,
                region,
                metricsList.First(x => x.region.Id == region.Id).regionMetrics.SimulationTimeSeconds,
                cancellationToken);
        }

        private Task<RegionMetrics> CalculateRegionMetrics(
            Region region,
            double hourFraction,
            double stepSeconds)
        {
            double consumed = IntegrateConsumption(region, hourFraction, stepSeconds);
            double produced = IntegrateProduction(region, hourFraction, stepSeconds);

            double stepHours = stepSeconds / 3600;

            double firstCategoryConsumed = consumed * (region.Consumer?.FirstPercentage ?? 0) / 100;
            double firstCategoryProduced = stepHours * region.PowerSource?.CalculateAvailableCapacityForFirstCategory() ?? 0;

            return Task.FromResult(new RegionMetrics
            {
                ConsumedEnergy = consumed,
                ProducedEnergy = produced,
                FirstCategoryDeficit = firstCategoryConsumed - firstCategoryProduced,
                RemainingDeficit = (consumed - firstCategoryConsumed) -
                                  (produced - Math.Min(firstCategoryProduced, firstCategoryConsumed))
            });
        }
        private async Task<bool> DistributePowerRecursivelyAsync(
    RegionDbContext context,
    Region region,
    HashSet<int> visited,
    double hourFraction,
    List<(Region region, RegionMetrics regionMetrics)> metricsList,
    bool isFirstCategoryDistributed,
    CancellationToken cancellationToken)
        {
            if (visited.Contains(region.Id))
            {
                return false;
            }
            visited.Add(region.Id);

            double stepSeconds = _speedFactor;

            var currentMetrics = metricsList.First(x => x.region.Id == region.Id).regionMetrics;
            if (currentMetrics.FirstCategoryDeficit <= 0 && currentMetrics.RemainingDeficit <= 0)
            {
                return true;
            }

            var connections = await context.ConnectedRegions
                .Where(cr => cr.RegionSourceId == region.Id)
                .ToListAsync(cancellationToken);

            foreach (var connection in connections)
            {
                if (currentMetrics.FirstCategoryDeficit <= 0 && currentMetrics.RemainingDeficit <= 0)
                {
                    return true;
                }
                if (visited.Contains(connection.RegionDestinationId))
                {
                    continue;
                }

                var neighbourRegion = await GetRegionWithDependenciesAsync(
                    context,
                    connection.RegionDestinationId,
                    cancellationToken);

                if (neighbourRegion == null || neighbourRegion.Consumer == null || neighbourRegion.PowerSource == null)
                {
                    continue;
                }

                var neighbourMetrics = metricsList.FirstOrDefault(x => x.region.Id == neighbourRegion.Id).regionMetrics;
                if (neighbourMetrics == null)
                {
                    Console.WriteLine($"NeighbourRegion {neighbourRegion.Id} not found in metricsList");
                    continue;
                }

                double neighbourHourFraction = CalculateNeighbourHourFraction(region, neighbourRegion, hourFraction);

                if (isFirstCategoryDistributed)
                {
                    if (currentMetrics.FirstCategoryDeficit > 0 && neighbourMetrics.FirstCategoryDeficit < 0)
                    {
                        double donorAvailable = -neighbourMetrics.FirstCategoryDeficit;
                        double required = currentMetrics.FirstCategoryDeficit;

                        neighbourMetrics.FirstCategoryDeficit += required;
                        currentMetrics.FirstCategoryDeficit = 0;

                        if (required <= donorAvailable ||
                            await DistributePowerRecursivelyAsync(
                                context,
                                neighbourRegion,
                                visited,
                                neighbourHourFraction,
                                metricsList,
                                isFirstCategoryDistributed,
                                cancellationToken))
                        {
                            await ExecuteTransfer(
                                context,
                                region.Id,
                                neighbourRegion.Id,
                                required,
                                isFirstCategory: true,
                                cancellationToken);
                        }
                        else
                        {
                            double actualTransferred = donorAvailable;
                            currentMetrics.FirstCategoryDeficit = required - donorAvailable;
                            neighbourMetrics.FirstCategoryDeficit = 0;

                            await ExecuteTransfer(
                                context,
                                region.Id,
                                neighbourRegion.Id,
                                actualTransferred,
                                isFirstCategory: true,
                                cancellationToken);
                        }
                    }
                    else if (currentMetrics.FirstCategoryDeficit > 0 && neighbourMetrics.FirstCategoryDeficit > 0)
                    {
                        bool success = await DistributePowerRecursivelyAsync(
                            context,
                            neighbourRegion,
                            visited,
                            neighbourHourFraction,
                            metricsList,
                            isFirstCategoryDistributed,
                            cancellationToken);

                        if (success && neighbourMetrics.FirstCategoryDeficit < 0)
                        {
                            double donorAvailable = -neighbourMetrics.FirstCategoryDeficit;
                            double required = currentMetrics.FirstCategoryDeficit;

                            neighbourMetrics.FirstCategoryDeficit += required;
                            currentMetrics.FirstCategoryDeficit = 0;

                            await ExecuteTransfer(
                                context,
                                region.Id,
                                neighbourRegion.Id,
                                required <= donorAvailable ? required : donorAvailable,
                                isFirstCategory: true,
                                cancellationToken);
                        }
                    }
                }
                else
                {
                    if (currentMetrics.RemainingDeficit > 0 && neighbourMetrics.RemainingDeficit < 0)
                    {
                        double donorAvailable = -neighbourMetrics.RemainingDeficit;
                        double required = currentMetrics.RemainingDeficit;

                        neighbourMetrics.RemainingDeficit += required;
                        currentMetrics.RemainingDeficit = 0;

                        if (required <= donorAvailable ||
                            await DistributePowerRecursivelyAsync(
                                context,
                                neighbourRegion,
                                visited,
                                neighbourHourFraction,
                                metricsList,
                                isFirstCategoryDistributed,
                                cancellationToken))
                        {
                            await ExecuteTransfer(
                                context,
                                region.Id,
                                neighbourRegion.Id,
                                required,
                                isFirstCategory: false,
                                cancellationToken);
                        }
                        else
                        {
                            double actualTransferred = donorAvailable;
                            currentMetrics.RemainingDeficit = required - donorAvailable;
                            neighbourMetrics.RemainingDeficit = 0;

                            await ExecuteTransfer(
                                context,
                                region.Id,
                                neighbourRegion.Id,
                                actualTransferred,
                                isFirstCategory: false,
                                cancellationToken);
                        }
                    }
                    else if (currentMetrics.RemainingDeficit > 0 && neighbourMetrics.RemainingDeficit > 0)
                    {
                        bool success = await DistributePowerRecursivelyAsync(
                            context,
                            neighbourRegion,
                            visited,
                            neighbourHourFraction,
                            metricsList,
                            isFirstCategoryDistributed,
                            cancellationToken);

                        if (success && neighbourMetrics.RemainingDeficit < 0)
                        {
                            double donorAvailable = -neighbourMetrics.RemainingDeficit;
                            double required = currentMetrics.RemainingDeficit;

                            neighbourMetrics.RemainingDeficit += required;
                            currentMetrics.RemainingDeficit = 0;

                            await ExecuteTransfer(
                                context,
                                region.Id,
                                neighbourRegion.Id,
                                required <= donorAvailable ? required : donorAvailable,
                                isFirstCategory: false,
                                cancellationToken);
                        }
                    }
                }
            }

            return currentMetrics.FirstCategoryDeficit <= 0 && currentMetrics.RemainingDeficit <= 0;
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
        private async Task SendConnectionUpdate(
            List<ConnectedRegion> connections,
            Region region,
            double simulationTimeSeconds,
            CancellationToken cancellationToken)
        {

            var connectionDtos = connections.Select(c => new
            {
                SourceRegionId = c.RegionSourceId,
                DestinationRegionId = c.RegionDestinationId,
                SentFirstCategoryCapacity = c.SentFirstCategoryCapacity,
                ReceivedFirstCategoryCapacity = c.ReceivedFirstCategoryCapacity,
                SentRemainingCapacity = c.SentRemainingCapacity,
                ReceivedRemainingCapacity = c.ReceivedRemainingCapacity
            }).ToList();
            await _hubContext.Clients.All.SendAsync("ReceiveConnectionUpdate", connectionDtos);
        }

        #region Calculate Hour Fraction
        private double CalculateNeighbourHourFraction(Region current, Region neighbor, double currentHourFraction)
        {
            int timeZoneDiff = neighbor.TimeZoneOffset - current.TimeZoneOffset;
            return (currentHourFraction + timeZoneDiff) % 24;
        }

        private double CalculateRegionalHourFraction(Region region, double baseHourFraction)
        {
            double fraction = (baseHourFraction + (region.TimeZoneOffset - _baseTimeZoneOffset)) % 24;
            return fraction < 0 ? fraction + 24 : fraction;
        }

        #endregion

        #region Integration Methods
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
        #endregion

        #region Simulation Control Methods
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

                await _hubContext.Clients.All.SendAsync("ReceiveSimulationStatus",
                    new { Status = "Stopped", Message = "Simulation stopped." });
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

                await _hubContext.Clients.All.SendAsync("ReceiveSimulationStatus",
                 new { Status = "Paused", Message = $"Simulation paused at {FormatTime(_currentTick * _speedFactor)}." });
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

                await _hubContext.Clients.All.SendAsync("ReceiveSimulationStatus",
                new { Status = "Resumed", Message = $"Simulation resumed at {FormatTime(_currentTick * _speedFactor)}." });
            }
            finally
            {
                _simulationLock.Release();
            }
        }
        #endregion

        #region Helper Methods
        private string FormatTime(double seconds)
        {
            int hours = (int)(seconds / 3600) % 24;
            int minutes = (int)((seconds % 3600) / 60);
            int secs = (int)(seconds % 60);
            return $"{hours:00}:{minutes:00}:{secs:00}";
        }
        private async Task CleanupSimulation()
        {
            _simulationTimer?.Dispose();
            _simulationTimer = null;
            _simulationCts?.Dispose();
            _simulationCts = null;
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
            new { RecipientId = recipientId, DonorId = donorId, Amount = amount, Category = category });
        }
        #endregion
    }
}
