using InteractiveStand.Domain.Classes;

namespace InteractiveStand.Application.Interfaces
{
    public interface IMqttSimulationPublisher
    {
        Task PublishRegionConsumerStatusAsync(ConsumerBinding consumerBinding, double currentTime, CancellationToken cancellationToken);
    }

}
