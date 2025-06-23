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
            try
            {
                await _powerDistributionService.StartSimulationAsync(speedFactor);
                return Ok($"Simulation started with speed factor {speedFactor}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("pause")]
        public async Task<IActionResult> PauseSimulation()
        {
            try
            {
                await _powerDistributionService.PauseSimulationAsync();
                return Ok("Simulation paused");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("resume")]
        public async Task<IActionResult> ResumeSimulation()
        {
            try
            {
                await _powerDistributionService.ResumeSimulationAsync();
                return Ok("Simulation resumed");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopSimulation()
        {
            try
            {
                await _powerDistributionService.StopSimulationAsync();
                return Ok("Simulation stopped");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("logs")]
        public IActionResult GetSimulationLogs()
        {
            var logs = _powerDistributionService.GetSimulationLogs();
            return Ok(new { Logs = logs });
        }
    }
}