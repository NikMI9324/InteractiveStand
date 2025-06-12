using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveStand.Domain.Classes
{
    public class Consumer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Range(0,100)]
        public double FirstPercentage { get; set; }
        [Required]
        [Range(0,100)]
        public double SecondPercentage { get; set; }
        [Required]
        [Range(0,100)]
        public double ThirdPercentage { get; set; }
        public Consumer() { }

        public Consumer(double firstPercentage, double secondPercentage, double thirdPercentage)
        {
            FirstPercentage = firstPercentage;
            SecondPercentage = secondPercentage;
            ThirdPercentage = thirdPercentage;
        }

    }
}
