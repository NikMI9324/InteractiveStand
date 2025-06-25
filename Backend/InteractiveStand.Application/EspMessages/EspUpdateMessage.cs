using System.Text.Json.Serialization;

namespace InteractiveStand.Application.EspMessages
{
    public class EspUpdateMessage
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }
        [JsonPropertyName("MAC")]
        public string Mac { get; set; } = string.Empty;

    }
}
