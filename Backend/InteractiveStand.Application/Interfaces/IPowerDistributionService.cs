using InteractiveStand.Domain.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveStand.Application.Interfaces
{
    public interface IPowerDistributionService
    {
        Task StartSimulationAsync(int speedFactor);
        Task StopSimulationAsync();
        Task PauseSimulationAsync();
        Task ResumeSimulationAsync();
        List<string> GetSimulationLogs();
    }
}
