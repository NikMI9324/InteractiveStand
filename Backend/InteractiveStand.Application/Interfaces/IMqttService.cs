
namespace InteractiveStand.Application.Interfaces
{
    public interface IMqttService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
