using InteractiveStand.Application.Dtos;
using InteractiveStand.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InteractiveStand.API.Controllers
{
    [ApiController]
    [Route("api/region")]
    public class RegionController : ControllerBase
    {
        private readonly IRegionService _regionService;
        private readonly IPowerDistributionService _powerDistributionService;
        public RegionController(IRegionService regionService, IPowerDistributionService powerDistributionService)
        {
            _regionService = regionService ?? throw new ArgumentNullException(nameof(regionService));
            _powerDistributionService = powerDistributionService ?? throw new ArgumentNullException(nameof(powerDistributionService));
        }

        [HttpGet("all-regions")]
        public async Task<IActionResult> GetRegions()
        {
            try
            {
                var regions = await _regionService.GetRegionsAsync();
                return Ok(regions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{regionId:int}")]
        public async Task<IActionResult> GetRegionById([FromRoute]int regionId)
        {
            try
            {
                var region = await _regionService.GetRegionByIdAsync(regionId);
                if (region == null)
                {
                    return NotFound($"Region with ID {regionId} not found.");
                }
                return Ok(region);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
        [HttpPut("update/powersource/{regionId:int}")]
        public async Task<IActionResult> UpdatePowerSource([FromRoute] int regionId, [FromBody] PowerSourceUpdateCapacityDto dto)
        {

            try
            {
                var updatedPercentages = await _regionService.AddCapacityPowerSource(regionId, dto);
                return Ok(updatedPercentages);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при обновлении источника энергии: {ex.Message}");
            }
        }
        
        [HttpPost("reset")]
        public async Task<IActionResult> ResetData()
        {
            await _regionService.ResetDataAsync();
            return Ok("Данные успешно сброшены.");
        }

    }
}
