using InteractiveStand.Application.EspMessages;

namespace InteractiveStand.Application.Interfaces
{
    public interface IUpdateMessageHandler
    {
        Task HandleAsync(EspUpdateMessage updateMessage, CancellationToken cancellationToken = default);
    }
}
