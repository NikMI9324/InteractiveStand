using InteractiveStand.Domain.Classes;
using InteractiveStand.Application.Dtos.RegionDto;
using InteractiveStand.Application.Dtos.PowerSourceDto;
using InteractiveStand.Application.Dtos.ConsumerDto;

namespace InteractiveStand.Application.Interfaces
{
    public interface IRegionService
    {
        Task<PowerSource> AddCapacityToPowerSource( int regionId, PowerSourceUpdateCapacityDto dto);
        Task<List<RegionGetDto>> GetRegionsAsync();
        Task AddConsumedCapacity(int regionId, double additionalConsumedCapacity);
        Task ChangeConsumersPercentage(int regionId, ConsumerUpdatePercantageDto dto);
        Task<RegionGetDto> GetRegionByIdAsync(int regionId);
        Task ResetDataAsync();
        Task<RegionUpdateDto> UpdateRegion(int regionId, RegionUpdateDto dto);
    }
}
