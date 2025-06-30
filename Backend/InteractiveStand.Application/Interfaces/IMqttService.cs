
using InteractiveStand.Domain.Classes;

namespace InteractiveStand.Application.Interfaces
{
    public interface IMqttService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        Task PublishProducedCapacityAsync(ProducerBinding producerBinding, PowerSource powerSource, CancellationToken cancellationToken);
    }
}
