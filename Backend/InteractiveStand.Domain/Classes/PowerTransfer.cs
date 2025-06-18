using System.ComponentModel.DataAnnotations;

namespace InteractiveStand.Domain.Classes
{
    public class PowerTransfer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int WhoSentId { get; set; }
        [Required]
        public int WhoReceivedId { get; set; }
        [Required]
        public double SentCapacity { get; set; }
        [Required]
        public string? Description { get; set; } = string.Empty;
    }
}
