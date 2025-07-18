﻿using InteractiveStand.Application.Dtos.ConsumerDto;
using InteractiveStand.Application.Dtos.PowerSourceDto;

namespace InteractiveStand.Application.Dtos.RegionDto
{
    public class RegionSimulationDto
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public string Name { get; set; }
        public double ConsumedEnergy { get; set; }
        public double ProducedEnergy { get; set; }
        public double FirstCategoryDeficit { get; set; }
        public double RemainingDeficit { get; set; }
        public double NPP_Percentage { get; set; }
        public double HPP_Percentage { get; set; }
        public double CGPP_Percentage { get; set; }
        public double WPP_Percentage { get; set; }
        public double SPP_Percentage { get; set; }
        public double FirstPercentage { get; set; }
        public double SecondPercentage { get; set; }
        public double ThirdPercentage { get; set; } 
        //public int TimeZoneOffset { get; set; }
    }
}
