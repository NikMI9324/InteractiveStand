using InteractiveStand.Application.EspMessages;
using InteractiveStand.Application.Hubs;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Domain.Classes;
using InteractiveStand.Domain.Enums;
using InteractiveStand.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InteractiveStand.Application.Services
{
    public class ConnectMessageHandler : IConnectMessageHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHubContext<EnergyDistributionHub> _hubContext;

        public ConnectMessageHandler(
            IServiceScopeFactory serviceScopeFactory, 
            IHubContext<EnergyDistributionHub> hubContext)
        {
            _serviceScopeFactory = serviceScopeFactory 
                ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _hubContext = hubContext 
                ?? throw new ArgumentNullException(nameof(hubContext));
        }
        public async Task HandleAsync(EspConnectMessage connectMessage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectMessage.Mac))
                return;

            if (connectMessage.RegionId < 0 || connectMessage.RegionId > 10)
                return;
            bool isConsumer = Enum.TryParse<CapacityConsumerType>(connectMessage.ModuleType, true, out var consumerModuleType);
            bool isProducer = Enum.TryParse<CapacityProducerType>(connectMessage.ModuleType, true, out var producerModuleType);
            if (string.IsNullOrWhiteSpace(connectMessage.ModuleType) || !isConsumer && !isProducer)
                return;

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<RegionDbContext>();

            var existingConsumerBinding = await dbContext.ConsumerBindings
                .FirstOrDefaultAsync(cb => cb.MacAddress == connectMessage.Mac, cancellationToken);
            var existingProducerBinding = await dbContext.ProducerBindings
                .FirstOrDefaultAsync(pb => pb.MacAddress == connectMessage.Mac, cancellationToken);

            ConsumerBinding? newConsumerBinding;
            ProducerBinding? newProducerBinding;
            if (connectMessage.RegionId != 0 && isConsumer)
            {
                newConsumerBinding = await dbContext.ConsumerBindings
                    .FirstOrDefaultAsync(cb => cb.RegionId == connectMessage.RegionId
                    && cb.CapacityConsumerType == consumerModuleType, cancellationToken);
                if (newConsumerBinding != null)
                    newConsumerBinding.MacAddress = connectMessage.Mac;
            }
            else if(connectMessage.RegionId != 0 && isProducer)
            {
                newProducerBinding = await dbContext.ProducerBindings
                    .FirstOrDefaultAsync(pb => pb.RegionId == connectMessage.RegionId
                    &&  pb.CapacityProducerType == producerModuleType, cancellationToken);
                if (newProducerBinding != null)
                    newProducerBinding.MacAddress = connectMessage.Mac;
            }

            if (existingConsumerBinding != null
                && (existingConsumerBinding.RegionId != connectMessage.RegionId
                || existingConsumerBinding.CapacityConsumerType != consumerModuleType))
            {
                if (connectMessage.RegionId == 0)
                {
                    newConsumerBinding = await dbContext.ConsumerBindings
                            .FirstOrDefaultAsync(cb => cb.MacAddress == string.Empty
                                                       && cb.CapacityConsumerType == consumerModuleType, cancellationToken);
                    if (newConsumerBinding != null)
                        newConsumerBinding.MacAddress = connectMessage.Mac;
                }
                existingConsumerBinding.MacAddress = string.Empty;
            }
            else if (existingProducerBinding != null
                     && (existingProducerBinding.RegionId != connectMessage.RegionId
                     || existingProducerBinding.CapacityProducerType != producerModuleType))
            {
                if (connectMessage.RegionId == 0)
                {
                    newProducerBinding = await dbContext.ProducerBindings
                   .FirstOrDefaultAsync(pb => pb.MacAddress == string.Empty
                                              && pb.CapacityProducerType == producerModuleType, cancellationToken);
                    if (newProducerBinding != null)
                        newProducerBinding.MacAddress = connectMessage.Mac;
                }
                existingProducerBinding.MacAddress = string.Empty;

            }
            else if (existingConsumerBinding == null && existingProducerBinding == null)
            {
                if (isConsumer && connectMessage.RegionId == 0)
                {
                    newConsumerBinding = await dbContext.ConsumerBindings
                        .FirstOrDefaultAsync(cb => cb.MacAddress == string.Empty
                                                    && cb.CapacityConsumerType == consumerModuleType, cancellationToken);
                    if (newConsumerBinding != null)
                        newConsumerBinding.MacAddress = connectMessage.Mac;
                }
                else if (isProducer && connectMessage.RegionId == 0)
                {
                    newProducerBinding = await dbContext.ProducerBindings
                        .FirstOrDefaultAsync(pb => pb.MacAddress == string.Empty
                                                    && pb.CapacityProducerType == producerModuleType, cancellationToken);
                    if (newProducerBinding != null)
                        newProducerBinding.MacAddress = connectMessage.Mac;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            await _hubContext.Clients.All.SendAsync("EspConnected", new
            {
                mac = connectMessage.Mac,
                type = connectMessage.ModuleType,
                regionId = connectMessage.RegionId,
                time = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}
