using InteractiveStand.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InteractiveStand.Domain.Classes
{
    public class PowerSource
    {
        public const double  NPP_EFFICIENCY = 0.80;
        public const double  HPP_EFFICIENCY = 0.50;
        public const double CGPP_EFFICIENCY = 0.40;
        public const double  WPP_EFFICIENCY = 0.25;
        public const double  SPP_EFFICIENCY = 0.15;

        [Key]
        public int Id { get; set; }

        public double NPP_Capacity { get; set; } = 0.0;
        public double HPP_Capacity { get; set; } = 0.0;
        public double CGPP_Capacity { get; set; } = 0.0;
        public double WPP_Capacity { get; set; } = 0.0;
        public double SPP_Capacity { get; set; } = 0.0;

        [Range(0, 100)]
        public double NPP_Percentage { get; set; } = 0.0;
        [Range(0, 100)]
        public double HPP_Percentage { get; set; } = 0.0;
        [Range(0, 100)]
        public double CGPP_Percentage { get; set; } = 0.0;
        [Range(0, 100)]
        public double WPP_Percentage { get; set; } = 0.0;
        [Range(0, 100)]
        public double SPP_Percentage { get; set; } = 0.0;


        [Range(0, 200)]
        public double  NPP_LoadFactor { get; set; } = 100;
        [Range(0, 200)]
        public double  HPP_LoadFactor { get; set; } = 100;
        [Range(0, 200)]
        public double CGPP_LoadFactor { get; set; } = 100;
        [Range(0, 200)]
        public double  WPP_LoadFactor { get; set; } = 100;
        [Range(0, 200)]
        public double  SPP_LoadFactor { get; set; } = 100;

        public double TotalCapacity => NPP_Capacity + HPP_Capacity + CGPP_Capacity + WPP_Capacity + SPP_Capacity;

        public double GetCurrentNPPCapacity() => NPP_Capacity * NPP_LoadFactor / 100.0;
        public double GetCurrentHPPCapacity() => HPP_Capacity * HPP_LoadFactor / 100.0;
        public double GetCurrentCGPPCapacity() => CGPP_Capacity * CGPP_LoadFactor / 100.0;
        public double GetCurrentWPPCapacity() => WPP_Capacity * WPP_LoadFactor / 100.0;
        public double GetCurrentSPPCapacity() => SPP_Capacity * SPP_LoadFactor / 100.0;

        public double TotalPercentage => NPP_Percentage + HPP_Percentage + CGPP_Percentage + WPP_Percentage + SPP_Percentage;

        public double TotalCurrentCapacity => GetCurrentNPPCapacity() + GetCurrentHPPCapacity() +
                                             GetCurrentCGPPCapacity() + GetCurrentWPPCapacity() +
                                             GetCurrentSPPCapacity();

        public PowerSource() { }

        public double CalculateAvailableCapacity()
        {
            return GetCurrentNPPCapacity() * NPP_EFFICIENCY +
                   GetCurrentHPPCapacity() * HPP_EFFICIENCY +
                   GetCurrentCGPPCapacity() * CGPP_EFFICIENCY +
                   GetCurrentWPPCapacity() * WPP_EFFICIENCY +
                   GetCurrentSPPCapacity() * SPP_EFFICIENCY;
        }
        public double GetProducerCapacity(CapacityProducerType producerType)
        {
            switch (producerType) 
            {
                case  CapacityProducerType.PROD_NPP: return  GetCurrentNPPCapacity();
                case  CapacityProducerType.PROD_HPP: return  GetCurrentHPPCapacity();
                case  CapacityProducerType.PROD_WPP: return  GetCurrentWPPCapacity();
                case CapacityProducerType.PROD_CGPP: return GetCurrentCGPPCapacity();
                case  CapacityProducerType.PROD_SPP: return  GetCurrentSPPCapacity();
                                                        default: return 0;
            }
        }
        public double CalculateAvailableCapacityForFirstCategory()
        {
            return GetCurrentNPPCapacity() * NPP_EFFICIENCY +
                   GetCurrentHPPCapacity() * HPP_EFFICIENCY +
                   GetCurrentCGPPCapacity() * CGPP_EFFICIENCY * 0.25;
        }
        public void RecalculatePercentages()
        {
            double totalCurrent = TotalCurrentCapacity;
            if (totalCurrent > 0)
            {
                NPP_Percentage = GetCurrentNPPCapacity() / totalCurrent * 100.0;
                HPP_Percentage = GetCurrentHPPCapacity() / totalCurrent * 100.0;
                CGPP_Percentage = GetCurrentCGPPCapacity() / totalCurrent * 100.0;
                WPP_Percentage = GetCurrentWPPCapacity() / totalCurrent * 100.0;
                SPP_Percentage = GetCurrentSPPCapacity() / totalCurrent * 100.0;

                double totalPercentage = NPP_Percentage + HPP_Percentage + CGPP_Percentage +
                                         WPP_Percentage + SPP_Percentage;
                if (Math.Abs(totalPercentage - 100.0) > 0.01)
                {
                    double factor = 100.0 / totalPercentage;
                    NPP_Percentage *= factor;
                    HPP_Percentage *= factor;
                    CGPP_Percentage *= factor;
                    WPP_Percentage *= factor;
                    SPP_Percentage *= factor;
                }
            }
            else
            {
                NPP_Percentage = HPP_Percentage = CGPP_Percentage = WPP_Percentage = SPP_Percentage = 0.0;
            }

        }


        public void UpdateCapacity(CapacityProducerType type, double deltaCapacity, bool reduce)
        {
            double change = reduce ? -deltaCapacity : deltaCapacity;
            switch (type)
            {
                case CapacityProducerType.PROD_NPP:
                    NPP_Capacity = Math.Max(0, NPP_Capacity + change);
                    break;
                case CapacityProducerType.PROD_HPP:
                    HPP_Capacity = Math.Max(0, HPP_Capacity + change);
                    break;
                case CapacityProducerType.PROD_CGPP:
                    CGPP_Capacity = Math.Max(0, CGPP_Capacity + change);
                    break;
                case CapacityProducerType.PROD_WPP:
                    WPP_Capacity = Math.Max(0, WPP_Capacity + change);
                    break;
                case CapacityProducerType.PROD_SPP:
                    SPP_Capacity = Math.Max(0, SPP_Capacity + change);
                    break;
            }
            RecalculatePercentages();
        }
        public void UpdateLoadFactor(CapacityProducerType type, double loadFactor)
        {
            loadFactor = Math.Clamp(loadFactor, 0, 200);
            switch (type)
            {
                case CapacityProducerType.PROD_NPP:
                    NPP_LoadFactor = loadFactor;
                    break;
                case CapacityProducerType.PROD_HPP:
                    HPP_LoadFactor = loadFactor;
                    break;
                case CapacityProducerType.PROD_CGPP:
                    CGPP_LoadFactor = loadFactor;
                    break;
                case CapacityProducerType.PROD_WPP:
                    WPP_LoadFactor = loadFactor;
                    break;
                case CapacityProducerType.PROD_SPP:
                    SPP_LoadFactor = loadFactor;
                    break;
            }
            RecalculatePercentages();
        }
    }
}
