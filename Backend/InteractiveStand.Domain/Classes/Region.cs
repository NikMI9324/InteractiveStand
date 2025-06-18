using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//Формулы на будущее
//S = 16,86374N
// N/4 + (3 * N / 4) * exp^(-((x-7)/4,25)^2) + (3 * N / 4) * exp^(-((x-17)/4,25)^2) 


namespace InteractiveStand.Domain.Classes
{
    public class Region
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Range(0, double.MaxValue)]
        public double ProducedCapacity { get; set; }
        [Range(0, double.MaxValue)]
        public double ConsumedCapacity { get; set; }
        public int PowerSourceId { get; set; }
        [NotMapped]
        public PowerSource? PowerSource { get; set; } 
        public int ConsumerId { get; set; }
        [NotMapped]
        public Consumer? Consumer { get; set; }
        public int TimeZoneOffset { get; set; } 
        [NotMapped]
        public double DailyConsumedCapacity => (ConsumedCapacity * 1000) / 365;
        [NotMapped]
        public double DailyPeakConsumedCapacity => DailyConsumedCapacity / 16.86374;
        [NotMapped]
        public double HourlyProducedCapacity => 
            PowerSource?.CalculateAvailableCapacity(ProducedCapacity) ?? 0;
        [NotMapped]
        public double FirstCategoryProducedCapacity => 
            PowerSource?.CalculateAvailableCapacityForFirstCategory(ProducedCapacity) ?? 0;
        [NotMapped]
        public double FirstCategoryConsumedCapacity => 
            DailyConsumedCapacity * (Consumer?.FirstPercentage ?? 0) / 100;
        [NotMapped]
        public double FirstCategoryDeficit => 
            FirstCategoryConsumedCapacity - FirstCategoryProducedCapacity;
        [NotMapped]
        public double RemainingProducedCapacity => HourlyProducedCapacity - 
            (FirstCategoryProducedCapacity >= FirstCategoryConsumedCapacity ? 
            FirstCategoryConsumedCapacity : FirstCategoryProducedCapacity);
        [NotMapped]
        public double RemainingConsumedCapacity => DailyConsumedCapacity - FirstCategoryConsumedCapacity;
        [NotMapped]
        public double RemainingDeficit => RemainingConsumedCapacity - RemainingProducedCapacity;

        public (bool IsEnoughForFirstCategory, bool IsEnoughRemaining) GetCapacityStatus()
        {
            if (PowerSource == null || Consumer == null)
            {
                return (false, false);
            }
            return (FirstCategoryDeficit <= 0, RemainingDeficit <= 0);
        }
    }
}