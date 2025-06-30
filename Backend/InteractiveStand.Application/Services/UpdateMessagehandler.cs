
using InteractiveStand.Application.EspMessages;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InteractiveStand.Application.Services
{
    public class UpdateMessagehandler : IUpdateMessageHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public UpdateMessagehandler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task HandleAsync(EspUpdateMessage updateMessage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(updateMessage.Mac) || updateMessage.Value < 0 || updateMessage.Value > 200)
                return;

            var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<RegionDbContext>();

            var producerBinding = await dbContext.ProducerBindings
                .FirstOrDefaultAsync(pb => pb.MacAddress == updateMessage.Mac, cancellationToken);
            if(producerBinding == null) return;

            var region = await dbContext.Regions
                .Include(r => r.PowerSource)
                .FirstOrDefaultAsync(r => r.Id == producerBinding.RegionId,cancellationToken);
            
            if(region?.PowerSource == null) return;

            region.PowerSource.UpdateLoadFactor(producerBinding.CapacityProducerType, updateMessage.Value);
            region.ProducedCapacity = region.PowerSource.TotalCurrentCapacity;

            await dbContext.SaveChangesAsync(cancellationToken);


        }
    }
}
