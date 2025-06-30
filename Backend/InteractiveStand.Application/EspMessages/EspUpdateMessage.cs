using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InteractiveStand.Application.EspMessages
{
    public class EspUpdateMessage
    {
        [JsonPropertyName("value")]
        [Range(0,200)]
        public int Value { get; set; }
        [JsonPropertyName("MAC")]
        public string Mac { get; set; } = string.Empty;

    }
}
