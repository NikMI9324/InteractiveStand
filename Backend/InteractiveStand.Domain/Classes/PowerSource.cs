using InteractiveStand.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InteractiveStand.Domain.Classes
{
    public class PowerSource
    {
        public const double AESEfficiency = 0.80;
        public const double GESEfficiency = 0.50;
        public const double TESEfficiency = 0.40;
        public const double   VESEffiency = 0.25;
        public const double SESEfficiency = 0.15;

        [Key]
        public int Id { get; set; }

        [Range(0, 100)]
        public double AESPercentage { get; set; } = 0.0;
        [Range(0, 100)]
        public double GESPercentage { get; set; } = 0.0;
        [Range(0, 100)]
        public double TESPercentage { get; set; } = 0.0;
        [Range(0, 100)]
        public double VESPercentage { get; set; } = 0.0;
        [Range(0, 100)]
        public double SESPercentage { get; set; } = 0.0;
        
        public double TotalPercentage => AESPercentage + GESPercentage + TESPercentage + VESPercentage + SESPercentage;
        public PowerSource() { }

        public double CalculateAvailableCapacity(double producedCapacity)
        {
            return AESPercentage * AESEfficiency * producedCapacity / 100 +
                   GESPercentage * GESEfficiency * producedCapacity / 100 +
                   TESPercentage * TESEfficiency * producedCapacity / 100 +
                   VESPercentage *   VESEffiency * producedCapacity / 100 +
                   SESPercentage * SESEfficiency * producedCapacity / 100;
        }
        public double CalculateAvailableCapacityForFirstCategory(double producedCapacity)
        {
            double gasTESPercentage = TESPercentage * 0.25;
            double availableCapacity = producedCapacity *    AESPercentage * AESEfficiency / 100 +
                                       producedCapacity *    GESPercentage * GESEfficiency / 100 +
                                       producedCapacity * gasTESPercentage * TESEfficiency / 100;
            return availableCapacity;
        }
        public void RecalculatePercentages(double currentCapacity, double additionalCapacity, GenerationType type, bool reduce)
        {
            if (reduce)
                additionalCapacity *= (-1);

            double newCapacity = currentCapacity + additionalCapacity;

            double currentAES = currentCapacity * AESPercentage / 100;
            double currentGES = currentCapacity * GESPercentage / 100;
            double currentTES = currentCapacity * TESPercentage / 100;
            double currentVES = currentCapacity * VESPercentage / 100;
            double currentSES = currentCapacity * SESPercentage / 100;
            switch (type)
            {
                case GenerationType.AES:
                    currentAES += additionalCapacity;
                    break;
                case GenerationType.GES:
                    currentGES += additionalCapacity;
                    break;
                case GenerationType.TES:
                    currentTES += additionalCapacity;
                    break;
                case GenerationType.VES:
                    currentVES += additionalCapacity;
                    break;
                case GenerationType.SES:
                    currentSES += additionalCapacity;
                    break;
            }
            AESPercentage = 100 * currentAES / newCapacity;
            GESPercentage = 100 * currentGES / newCapacity;
            TESPercentage = 100 * currentTES / newCapacity;
            VESPercentage = 100 * currentVES / newCapacity;
            SESPercentage = 100 * currentSES / newCapacity;

            double totalPercentage = AESPercentage + GESPercentage + TESPercentage + VESPercentage + SESPercentage;
            if (Math.Abs(totalPercentage - 100) > 0.01)
            {
                double factor = 100 / totalPercentage;
                AESPercentage *= factor;
                GESPercentage *= factor;
                TESPercentage *= factor;
                VESPercentage *= factor;
                SESPercentage *= factor;
            }
        }

    }
}
