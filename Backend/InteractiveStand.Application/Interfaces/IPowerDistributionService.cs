
namespace InteractiveStand.Application.Interfaces
{
    public interface IPowerDistributionService
    {
        Task StartSimulationAsync(int speedFactor);
        Task StopSimulationAsync();
        Task PauseSimulationAsync();
        Task ResumeSimulationAsync();
    }
}
