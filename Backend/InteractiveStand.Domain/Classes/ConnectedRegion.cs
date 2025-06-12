using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveStand.Domain.Classes
{
    public class ConnectedRegion
    {
        public int Id { get; set; }
        public int RegionSourceId { get; set; }
        public int RegionDestinationId { get; set;}
    }
}
