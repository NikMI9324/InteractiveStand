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

        [Range(0,100)]
        public double AESPercentage { get; set; }
        [Range(0, 100)]
        public double GESPercentage { get; set; }
        [Range(0, 100)]
        public double TESPercentage { get; set; }
        [Range(0, 100)]
        public double VESPercentage { get; set; }
        [Range(0, 100)]
        public double SESPercentage { get; set; }
        
        public double TotalPercentage => AESPercentage + GESPercentage + TESPercentage + VESPercentage + SESPercentage;
        public PowerSource() { }
        public PowerSource(double aesPercentage = 0.0, double gesPercentage = 0.0, double tesPercentage = 0.0,
                    double vesPercentage = 0.0, double sesPercentage = 0.0)
        {
            AESPercentage = aesPercentage;
            GESPercentage = gesPercentage;
            TESPercentage = tesPercentage;
            VESPercentage = vesPercentage;
            SESPercentage = sesPercentage;
        }

        public double CalculateAvailableCapacity(double producedCapacity)
        {
            return AESPercentage * AESEfficiency * producedCapacity / 100 +
                   GESPercentage * GESPercentage * producedCapacity / 100 +
                   TESPercentage * TESPercentage * producedCapacity / 100 +
                   VESPercentage * VESPercentage * producedCapacity / 100 +
                   SESPercentage * SESPercentage * producedCapacity / 100;
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
