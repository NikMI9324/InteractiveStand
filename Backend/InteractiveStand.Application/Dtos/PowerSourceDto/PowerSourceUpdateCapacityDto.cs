using InteractiveStand.Domain.Enums;

namespace InteractiveStand.Application.Dtos.PowerSourceDto
{
    public class PowerSourceUpdateCapacityDto
    {
        public double Capacity { get; set; }
        public CapacityProducerType Type { get; set; }
        public bool Reduce { get; set; }
    }
}
