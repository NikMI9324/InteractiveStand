using InteractiveStand.Domain.Classes;

namespace InteractiveStand.Domain.Interfaces
{
    public interface IRegionRepository
    {
        Task<Region> GetRegionByIdAsync(int regionId);
        Task<PowerSource> GetPowerSourceByIdAsync(int powerSourceId);
        Task<Consumer> GetComsumerByIdAsync(int consumerId);
        Task UpdateRegionAsync(Region region);
        Task UpdatePowerSourceAsync(PowerSource powerSource);
        Task UpdateConsumerAsync(Consumer consumer);
        Task<List<Region>> GetAllRegionsAsync();
        Task ResetDataAsync();
        Task ResetConnectedRegionCapacityValuesAsync();
        Task<List<ProducerBinding>> GetProducerBindingsWithRegionAsync(int regionId, CancellationToken token);
    }
}
