using InteractiveStand.Domain.Classes;

namespace InteractiveStand.Application.Interfaces
{
    public interface IMqttBackgroundPublisher
    {
        Task PublishProducedCapacityAsync(ProducerBinding producerBinding, PowerSource powerSource, CancellationToken cancellationToken);
    }
}
