using InteractiveStand.Domain.Classes;
using Microsoft.AspNetCore.SignalR;

namespace InteractiveStand.Application.Hubs
{
    public class EnergyDistributionHub : Hub
    {
        public async Task SendSimulationUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveSimulationUpdate", message);
        }
        public async Task SendDistributionUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveDistributionUpdate", message);
        }

        public async Task SendConnectionUpdate(List<ConnectedRegion> connections)
        {
            await Clients.All.SendAsync("ReceiveConnectionUpdate", connections);
        }
        public async Task SendRegionUpdate(List<Region> regions)
        {
            await Clients.All.SendAsync("ReceiveRegionUpdate", regions);
        }
    }
}
