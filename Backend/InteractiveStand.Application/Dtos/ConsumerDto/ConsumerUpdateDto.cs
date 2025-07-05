using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveStand.Application.Dtos.ConsumerDto
{
    public class ConsumerUpdateDto
    {
        public int Id { get; set; }
        public double FirstPercentage { get; set; }
        public double SecondPercentage { get; set; }
        public double ThirdPercentage { get; set; }
    }
}
