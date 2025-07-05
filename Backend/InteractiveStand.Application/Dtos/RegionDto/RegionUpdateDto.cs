using InteractiveStand.Application.Dtos.ConsumerDto;
using InteractiveStand.Application.Dtos.PowerSourceDto;

namespace InteractiveStand.Application.Dtos.RegionDto
{
    public class RegionUpdateDto
    {
        public double ConsumedCapacity { get; set; }
        public PowerSourceUpdateDto PowerSource { get; set; }
        public ConsumerUpdateDto Consumer { get; set; }
    }
}
