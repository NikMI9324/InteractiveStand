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
        public bool IsActive { get; set; } = true;
        [NotMapped]
        public double DailyConsumedCapacity => (ConsumedCapacity * 1000) / 365;
        [NotMapped]
        public double HourlyPeakConsumedCapacity => DailyConsumedCapacity / 16.86374;
        [NotMapped]
        public double HourlyProducedCapacity => 
            PowerSource?.CalculateAvailableCapacity() ?? 0;
        public double CalculateHourlyConsumption(double hourFraction)
        {
            double x = hourFraction;
            double term1 = HourlyPeakConsumedCapacity / 4;
            double term2 = (3 * HourlyPeakConsumedCapacity / 4) * Math.Exp(-Math.Pow((x - 7) / 4.25, 2));
            double term3 = (3 * HourlyPeakConsumedCapacity / 4) * Math.Exp(-Math.Pow((x - 17) / 4.25, 2));
            return term1 + term2 + term3;
        }
    }
}