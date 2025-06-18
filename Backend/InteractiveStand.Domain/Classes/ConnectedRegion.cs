
namespace InteractiveStand.Domain.Classes
{
    public class ConnectedRegion
    {
        public int Id { get; set; }
        public int RegionSourceId { get; set; }
        public int RegionDestinationId { get; set; }
        public double SentFirstCategoryCapacity { get; set; } = 0.0;
        public double SentRemainingCapacity { get; set; } = 0.0;
        public double ReceivedFirstCategoryCapacity { get; set; } = 0.0;
        public double ReceivedRemainingCapacity { get; set; } = 0.0;

    }
}
