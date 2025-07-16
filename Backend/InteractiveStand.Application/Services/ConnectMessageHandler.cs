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
        private readonly Dictionary<CapacityConsumerType, string> _consumerTypeNames = new()
        {
            { CapacityConsumerType.CON_FIRST, "Первая категория" },
            { CapacityConsumerType.CON_OTHER, "Остальные категории" }
        };
        private readonly Dictionary<CapacityProducerType, string> _producerTypeNames = new()
        {
            { CapacityProducerType.PROD_CGPP, "ТЭС" },
            { CapacityProducerType.PROD_NPP, "АЭС" },
            { CapacityProducerType.PROD_HPP, "ГЭС" },
            { CapacityProducerType.PROD_WPP, "ВЭС" },
            { CapacityProducerType.PROD_SPP, "СЭС" }
        };

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
            bool bindingChanged = false;
            if (isConsumer && existingProducerBinding != null)
            {
                await _hubContext.Clients.All.SendAsync("EspUpdated",
                    $"Привязка MAC-Адреса {existingProducerBinding.MacAddress}" +
                    $" была удалена с производителя из {existingProducerBinding.RegionId}-го региона " +
                    $" типа : {_producerTypeNames[existingProducerBinding.CapacityProducerType]} ", cancellationToken);

                existingProducerBinding.MacAddress = string.Empty;
            }
            else if (isProducer && existingConsumerBinding != null)
            {
                await _hubContext.Clients.All.SendAsync("EspUpdated",
                    $"Привязка MAC-Адреса  {existingConsumerBinding.MacAddress}" +
                    $" была удалена с потребителя из {existingConsumerBinding.RegionId}-го региона" +
                    $" типа : {_consumerTypeNames[existingConsumerBinding.CapacityConsumerType]} ", cancellationToken);
                existingConsumerBinding.MacAddress = string.Empty;
            }

            if (isConsumer)
                await ProcessConsumerBinding(
                    dbContext, 
                    connectMessage, 
                    consumerModuleType, 
                    existingConsumerBinding, 
                    cancellationToken);
            else if (isProducer)
                await ProcessProducerBinding(
                    dbContext, 
                    connectMessage, 
                    producerModuleType, 
                    existingProducerBinding, 
                    cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            
        }
        private async Task ProcessConsumerBinding(
            RegionDbContext dbContext,
            EspConnectMessage message,
            CapacityConsumerType consumerType,
            ConsumerBinding? existingBinding,
            CancellationToken cancellationToken)
        {
            if (existingBinding != null &&
                existingBinding.CapacityConsumerType == consumerType &&
                existingBinding.RegionId == message.RegionId)
            {
                return;
            }

            if (existingBinding != null)
            {
                await _hubContext.Clients.All.SendAsync("EspUpdated",
                    $"Привязка MAC-Адреса  {existingBinding.MacAddress}" +
                    $" была удалена с потребителя из {existingBinding.RegionId}-го региона" +
                    $" типа : {_consumerTypeNames[existingBinding.CapacityConsumerType]} ", cancellationToken);
                existingBinding.MacAddress = string.Empty;
            }

            var newBinding = message.RegionId == 0
                ? await dbContext.ConsumerBindings
                    .FirstOrDefaultAsync(c => c.MacAddress == string.Empty &&
                                            c.CapacityConsumerType == consumerType,
                                      cancellationToken)
                : await dbContext.ConsumerBindings
                    .FirstOrDefaultAsync(c => c.RegionId == message.RegionId &&
                                            c.CapacityConsumerType == consumerType,
                                      cancellationToken);

            if(newBinding == null)
                newBinding = await dbContext.ConsumerBindings
                    .FirstOrDefaultAsync(c => c.CapacityConsumerType == consumerType, cancellationToken);

            if (newBinding != null && newBinding.MacAddress != message.Mac)
            {
                newBinding.MacAddress = message.Mac;
                await _hubContext.Clients.All.SendAsync("EspUpdated",
                    $"Привязка MAC-Адреса  {newBinding.MacAddress}" +
                    $" была установлена на потребитель из {newBinding.RegionId}-го региона" +
                    $" типа : {_consumerTypeNames[newBinding.CapacityConsumerType]} ", cancellationToken);
            }    

        }

        private async Task ProcessProducerBinding(
            RegionDbContext dbContext,
            EspConnectMessage message,
            CapacityProducerType producerType,
            ProducerBinding? existingBinding,
            CancellationToken cancellationToken)
        {
            if (existingBinding != null &&
                existingBinding.CapacityProducerType == producerType &&
                existingBinding.RegionId == message.RegionId)
            {
                return;
            }
            if (existingBinding != null)
            {
                await _hubContext.Clients.All.SendAsync("EspUpdated",
                    $"Привязка MAC-Адреса  {existingBinding.MacAddress}" +
                    $" была удалена с производителя из {existingBinding.RegionId}-го региона" +
                    $" типа : {_producerTypeNames[existingBinding.CapacityProducerType]} ", cancellationToken);
                existingBinding.MacAddress = string.Empty;
            }

            var newBinding = message.RegionId == 0
                ? await dbContext.ProducerBindings
                    .FirstOrDefaultAsync(p => p.MacAddress == string.Empty &&
                                            p.CapacityProducerType == producerType,
                                      cancellationToken)
                : await dbContext.ProducerBindings
                    .FirstOrDefaultAsync(p => p.RegionId == message.RegionId &&
                                            p.CapacityProducerType == producerType,
                                      cancellationToken);

            if (newBinding == null)
                newBinding = await dbContext.ProducerBindings
                    .FirstOrDefaultAsync(p => p.CapacityProducerType == producerType, cancellationToken);

            if (newBinding != null && newBinding.MacAddress != message.Mac)
            {
                newBinding.MacAddress = message.Mac;
                await _hubContext.Clients.All.SendAsync("EspUpdated",
                    $"Привязка MAC-Адреса  {newBinding.MacAddress}" +
                    $" была установлена на производитель из {newBinding.RegionId}-го региона" +
                    $" типа : {_producerTypeNames[newBinding.CapacityProducerType]} ", cancellationToken);
            }
        }
    }
}
