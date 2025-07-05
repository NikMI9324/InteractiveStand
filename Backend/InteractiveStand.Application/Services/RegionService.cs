using AutoMapper;
using InteractiveStand.Application.Dtos.ConsumerDto;
using InteractiveStand.Application.Dtos.PowerSourceDto;
using InteractiveStand.Application.Dtos.RegionDto;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Application.Mapper;
using InteractiveStand.Domain.Classes;
using InteractiveStand.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace InteractiveStand.Application.Services
{
    public class RegionService : IRegionService
    {
        private readonly IRegionRepository _regionRepo;
        private readonly IMapper _mapper;
        public RegionService(IRegionRepository regionRepo, IMapper mapper)
        {
            _regionRepo = regionRepo;
            _mapper = mapper;
        }
        public async Task<PowerSource> AddCapacityToPowerSource( int regionId, PowerSourceUpdateCapacityDto dto)
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
            powerSource.UpdateCapacity(dto.Type, dto.Capacity, dto.Reduce);
            region.ProducedCapacity = powerSource.TotalCurrentCapacity;
            await _regionRepo.UpdatePowerSourceAsync(powerSource);
            await _regionRepo.UpdateRegionAsync(region);
            return powerSource;
        }
        public async Task<List<RegionGetDto>> GetRegionsAsync()
        {
            var regions = await _regionRepo.GetAllRegionsAsync();
            return _mapper.Map<List<RegionGetDto>>(regions);
        }

        public async Task AddConsumedCapacity(int regionId, double additionalConsumedCapacity)
        {
            var region = await _regionRepo.GetRegionByIdAsync(regionId);
            if(region is null)
            {
                throw new KeyNotFoundException($"Region with ID {regionId} not found.");
            }
            region.ConsumedCapacity += additionalConsumedCapacity;
            await _regionRepo.UpdateRegionAsync(region);
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

        public async Task<RegionGetDto> GetRegionByIdAsync(int regionId)
        {
            var region = await _regionRepo.GetRegionByIdAsync(regionId);
            if (region == null)
                throw new KeyNotFoundException($"Region with ID {regionId} not found.");
            return _mapper.Map<RegionGetDto>(region);
        }

        public async Task ResetDataAsync()
        {
            await _regionRepo.ResetDataAsync();
        }

        public async Task<RegionUpdateDto> UpdateRegion(int regionId, RegionUpdateDto dto)
        {
            var region = await _regionRepo.GetRegionByIdAsync(regionId);
            if (region == null)
            {
                throw new Exception($"Регион с id {regionId} не найден");
            }
            region.ConsumedCapacity = dto.ConsumedCapacity;
            if (region.PowerSource != null && dto.PowerSource != null && region.PowerSource.Id == dto.PowerSource.Id)
            {
                region.PowerSource.NPP_Capacity = dto.PowerSource.NPP_Capacity;
                region.PowerSource.HPP_Capacity = dto.PowerSource.HPP_Capacity;
                region.PowerSource.CGPP_Capacity = dto.PowerSource.CGPP_Capacity;
                region.PowerSource.WPP_Capacity = dto.PowerSource.WPP_Capacity;
                region.PowerSource.SPP_Capacity = dto.PowerSource.SPP_Capacity;
                region.ProducedCapacity = region.PowerSource.HPP_Capacity + region.PowerSource.CGPP_Capacity +
                    region.PowerSource.NPP_Capacity + region.PowerSource.SPP_Capacity + region.PowerSource.WPP_Capacity;
                region.PowerSource.RecalculatePercentages();
            }
            if (region.Consumer != null && dto.Consumer != null && region.Consumer.Id == dto.Consumer.Id)
            {
                region.Consumer.FirstPercentage = dto.Consumer.FirstPercentage;
                region.Consumer.SecondPercentage = dto.Consumer.SecondPercentage;
                region.Consumer.ThirdPercentage = dto.Consumer.ThirdPercentage;
            }
            await _regionRepo.UpdateRegionAsync(region);
            return _mapper.Map<RegionUpdateDto>(region);

        }
    }
}
