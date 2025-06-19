using InteractiveStand.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InteractiveStand.API.Controllers
{
    [ApiController]
    [Route("api/energy")]
    public class EnergyController : ControllerBase
    {
        private readonly IPowerDistributionService _powerDistributionService;
        public EnergyController(IPowerDistributionService powerDistributionService)
        {
            _powerDistributionService = powerDistributionService ?? throw new ArgumentNullException(nameof(powerDistributionService));
        }
        [HttpPost("simulate/{speedFactor}")]
        public async Task<IActionResult> StartSimulation(int speedFactor, CancellationToken cancellationToken = default)
        {
            await _powerDistributionService.StartSimulationAsync(speedFactor, cancellationToken);
            return Ok("Simulation started");
        }
        [HttpPost("stop")]
        public async Task<IActionResult> StopSimulation()
        {
            await _powerDistributionService.StopSimulationAsync();
            return Ok("Simulation stopped");
        }
        
        [HttpGet("logs")]
        public IActionResult GetSimulationLogs()
        {
            var logs = _powerDistributionService.GetSimulationLogs();
            return Ok(new { Logs = logs });
        }
    }
}
