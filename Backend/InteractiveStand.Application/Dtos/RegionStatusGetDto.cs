using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveStand.Application.Dtos
{
    public class RegionStatusGetDto
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;
        public double RegionFirstCategoryDeficit { get; set; }
        public double RegionRemainingDeficit { get; set; }

    }
}
