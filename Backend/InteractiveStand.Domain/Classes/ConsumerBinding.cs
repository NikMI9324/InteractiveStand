using InteractiveStand.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveStand.Domain.Classes
{
    public class ConsumerBinding
    {
        public int Id { get; set; }
        public string MacAddress { get; set; } = string.Empty;
        public CapacityConsumerType CapacityConsumerType { get; set; }
        public int RegionId { get; set; }
        [NotMapped]
        public Region? Region {  get; set; }
    }
}
