using AutoMapper;
using InteractiveStand.Application.Dtos.ConsumerDto;
using InteractiveStand.Application.Dtos.PowerSourceDto;
using InteractiveStand.Application.Dtos.RegionDto;
using InteractiveStand.Application.RegionMetricsClasses;
using InteractiveStand.Domain.Classes;

namespace InteractiveStand.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {
            CreateMap<Region, RegionGetDto>()
                //.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.PowerSource, opt => opt.MapFrom(src => src.PowerSource))
                .ForMember(dest => dest.Consumer, opt => opt.MapFrom(src => src.Consumer));
            CreateMap<Region, RegionUpdateDto>()
                .ForMember(dest => dest.PowerSource, opt => opt.MapFrom(src => src.PowerSource))
                .ForMember(dest => dest.Consumer, opt => opt.MapFrom(src => src.Consumer));

            CreateMap<PowerSource, PowerSourceGetDto>();
            CreateMap<PowerSource, PowerSourceUpdateDto>();


            CreateMap<(Region region, RegionMetrics metrics), RegionSimulationDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.region.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.region.IsActive))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.region.Name))
                .ForMember(dest => dest.NPP_Percentage, opt => opt.MapFrom(src => src.region.PowerSource.NPP_Percentage))
                .ForMember(dest => dest.HPP_Percentage, opt => opt.MapFrom(src => src.region.PowerSource.HPP_Percentage))
                .ForMember(dest => dest.CGPP_Percentage, opt => opt.MapFrom(src => src.region.PowerSource.CGPP_Percentage))
                .ForMember(dest => dest.WPP_Percentage, opt => opt.MapFrom(src => src.region.PowerSource.WPP_Percentage))
                .ForMember(dest => dest.SPP_Percentage, opt => opt.MapFrom(src => src.region.PowerSource.SPP_Percentage))

                .ForMember(dest => dest.ProducedEnergy, opt => opt.MapFrom(src => src.metrics.ProducedEnergy))
                .ForMember(dest => dest.ConsumedEnergy, opt => opt.MapFrom(src => src.metrics.ConsumedEnergy))
                .ForMember(dest => dest.FirstCategoryDeficit, opt => opt.MapFrom(src => src.metrics.FirstCategoryDeficit > 0 ? src.metrics.FirstCategoryDeficit : 0))
                .ForMember(dest => dest.RemainingDeficit, opt => opt.MapFrom(src => src.metrics.RemainingDeficit > 0 ? src.metrics.RemainingDeficit : 0));

            CreateMap<Consumer, ConsumerGetDto>();
            CreateMap<Consumer, ConsumerUpdateDto>();
        }
    }
}
