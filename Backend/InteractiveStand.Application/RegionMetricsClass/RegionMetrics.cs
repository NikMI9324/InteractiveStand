using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveStand.Application.RegionMetricsClass
{
    public class RegionMetrics
    {
        public double ConsumedEnergy { get; set; }
        public double ProducedEnergy { get; set; }
        public double FirstCategoryDeficit { get; set; }
        public double RemainingDeficit { get; set; }
        public double SimulationTimeSeconds { get; set; }
    }
}
