using InteractiveStand.Domain.Classes;

namespace InteractiveStand.Domain.Interfaces
{
    public interface IRegionRepository
    {
        Task<Region> GetRegionByIdAsync(int regionId);
        Task<PowerSource> GetPowerSourceByIdAsync(int id);
        Task<Consumer> GetComsumerByIdAsync(int id);
        Task UpdateRegion(Region region);
        Task UpdatePowerSourceAsync(PowerSource powerSource);
        Task UpdateConsumerAsync(Consumer consumer);
        Task<List<Region>> GetAllRegionsAsync();
        Task<Region> GetFullInfoRegionByIdAsync(int regionId);
        Task ResetDataAsync();
    }
}
