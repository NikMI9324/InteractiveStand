using InteractiveStand.Application.Dtos.ConsumerDto;
using InteractiveStand.Application.Dtos.PowerSourceDto;

namespace InteractiveStand.Application.Dtos.RegionDto
{
    public class RegionGetDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double ProducedCapacity { get; set; }
        public double ConsumedCapacity { get; set; }
        public int PowerSourceId { get; set; }
        public PowerSourceGetDto PowerSource { get; set; }
        public int ConsumerId { get; set; }
        public ConsumerGetDto Consumer { get; set; }
        public int TimeZoneOffset { get; set; }
        public bool IsActive { get; set; }
    }
}
