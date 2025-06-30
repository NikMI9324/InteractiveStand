using InteractiveStand.Application.Dtos;
using InteractiveStand.Application.RegionMetricsClass;
using InteractiveStand.Domain.Classes;

namespace InteractiveStand.Application.Mapper
{
    public static class RegionMapper
    {
        public static RegionMetricsDto ToRegionMetricsDto(this Region region, RegionMetrics metrics, double simulationTimeSeconds)
        {
            return new RegionMetricsDto
            {
                RegionId = region.Id,
                Name = region.Name,
                IsActive = region.IsActive,
                ConsumedEnergy = metrics.ConsumedEnergy,
                ProducedEnergy = metrics.ProducedEnergy,
                FirstCategoryDeficit = metrics.FirstCategoryDeficit <= 0 ? 0 : metrics.FirstCategoryDeficit,
                RemainingDeficit = metrics.RemainingDeficit <= 0 ? 0 : metrics.RemainingDeficit,
                SimulationTimeSeconds = simulationTimeSeconds,
                NPP_Percentage = region.PowerSource?.NPP_Percentage ?? 0,
                HPP_Percentage = region.PowerSource?.HPP_Percentage ?? 0,
                CGPP_Percentage = region.PowerSource?.CGPP_Percentage ?? 0,
                WPP_Percentage = region.PowerSource?.WPP_Percentage ?? 0,
                SPP_Percentage = region.PowerSource?.SPP_Percentage ?? 0,
                FirstPercentage = region.Consumer?.FirstPercentage ?? 0,
                SecondPercentage = region.Consumer?.SecondPercentage ?? 0,
                ThirdPercentage = region.Consumer?.ThirdPercentage ?? 0
            };
        }
    }
}
