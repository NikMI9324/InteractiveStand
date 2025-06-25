using InteractiveStand.Application.Dtos;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Application.Mapper;
using InteractiveStand.Domain.Classes;
using InteractiveStand.Domain.Interfaces;

namespace InteractiveStand.Application.Services
{
    public class RegionService : IRegionService
    {
        private readonly IRegionRepository _regionRepo;
        public RegionService(IRegionRepository regionRepo)
        {
            _regionRepo = regionRepo;
        }
        public async Task<PowerSource> AddCapacityPowerSource( int regionId, PowerSourceUpdateCapacityDto dto)
        {
            var region = await _regionRepo.GetRegionByIdAsync(regionId);
            if (region is null)
            {
                throw new KeyNotFoundException($"Region with ID {regionId} not found.");
            }
            var powerSource = await _regionRepo.GetPowerSourceByIdAsync(region.PowerSourceId);
            if (powerSource is null)
            {
                throw new KeyNotFoundException($"Power source with type {dto.Type} not found.");
            }
            powerSource.RecalculatePercentages(region.ProducedCapacity, dto.Capacity, dto.Type, dto.Reduce);
            await _regionRepo.UpdatePowerSourceAsync(powerSource);
            return powerSource;
        }
        public async Task<List<Region>> GetRegionsAsync()
        {
            return await _regionRepo.GetAllRegionsAsync();
        }

        public async Task AddConsumedCapacity(int regionId, double additionalConsumedCapacity)
        {
            var region = await _regionRepo.GetRegionByIdAsync(regionId);
            if(region is null)
            {
                throw new KeyNotFoundException($"Region with ID {regionId} not found.");
            }
            region.ConsumedCapacity += additionalConsumedCapacity;
            await _regionRepo.UpdateRegion(region);
        }

        public async Task ChangeConsumersPercentage(int regionId, ConsumerUpdatePercantageDto dto)
        {
            var region = await _regionRepo.GetRegionByIdAsync(regionId);
            if(region is null)
            {
                throw new KeyNotFoundException($"Region with ID {regionId} not found.");
            }
            var consumer = await _regionRepo.GetComsumerByIdAsync(region.ConsumerId);
            if(consumer is null)
            {
                throw new KeyNotFoundException($"Consumer with ID {region.ConsumerId} not found.");
            }

            consumer.FirstPercentage = dto.FirstPercentage;
            consumer.SecondPercentage = dto.SecondPercentage;
            consumer.ThirdPercentage = dto.ThirdPercentage;

            await _regionRepo.UpdateConsumerAsync(consumer);
        }

        public async Task<Region> GetRegionByIdAsync(int regionId)
        {
            var region = await _regionRepo.GetRegionByIdAsync(regionId);
            if(region is null)
                throw new KeyNotFoundException($"Region with ID {regionId} not found.");
            return region;
        }

        public async Task ResetDataAsync()
        {
            await _regionRepo.ResetDataAsync();
        }
    }
}
