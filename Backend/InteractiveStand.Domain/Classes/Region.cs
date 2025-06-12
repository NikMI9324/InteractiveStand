using InteractiveStand.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveStand.Domain.Classes
{
    public class Region
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Range(0, double.MaxValue)]
        public double ProducedCapacity { get; set; }
        [Range(0, double.MaxValue)]
        public double ConsumedCapacity { get; set; }
        
        public int PowerSourceId { get; set; }
        public int ConsumerId { get; set; }

        
    }
}