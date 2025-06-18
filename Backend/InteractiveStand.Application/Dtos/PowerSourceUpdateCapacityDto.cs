using InteractiveStand.Domain.Enums;

namespace InteractiveStand.Application.Dtos
{
    public class PowerSourceUpdateCapacityDto
    {
        public double Capacity { get; set; }
        public GenerationType Type { get; set; }
        public bool Reduce { get; set; }
    }
}
