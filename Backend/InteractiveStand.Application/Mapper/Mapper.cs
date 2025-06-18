using InteractiveStand.Application.Dtos;
using InteractiveStand.Domain.Classes;

namespace InteractiveStand.Application.Mapper
{
    public static class Mapper
    {
        public static RegionStatusGetDto ToRegionStatusDto(this Region region)
        {
            return new RegionStatusGetDto
            {
                RegionId = region.Id,
                RegionName = region.Name,
                RegionFirstCategoryDeficit = region.FirstCategoryDeficit,
                RegionRemainingDeficit = region.RemainingDeficit
            };
        }
    }
}
