using InteractiveStand.Domain.Classes;
using InteractiveStand.Application.Dtos;

namespace InteractiveStand.Application.Interfaces
{
    public interface IRegionService
    {
        Task<PowerSource> AddCapacityPowerSource( int regionId, PowerSourceUpdateCapacityDto dto);
        Task<List<Region>> GetRegionsAsync();
        Task<List<RegionStatusGetDto>> GetProblemRegionStatus();
        Task AddConsumedCapacity(int regionId, double additionalConsumedCapacity);
        Task ChangeConsumersPercentage(int regionId, ConsumerUpdatePercantageDto dto);
        Task<Region> GetRegionByIdAsync(int regionId);
        Task ResetDataAsync();
    }
}
