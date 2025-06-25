
using InteractiveStand.Application.EspMessages;

namespace InteractiveStand.Application.Interfaces
{
    public interface IConnectMessageHandler
    {
        Task HandleAsync(EspConnectMessage connectMessage, CancellationToken cancellationToken = default);
    }
}
