using System.Text.Json.Serialization;

namespace InteractiveStand.Application.EspMessages
{
    public class EspConnectMessage
    {
        [JsonPropertyName("module_type")]
        public string ModuleType { get; set; } = string.Empty;

        [JsonPropertyName("MAC")]
        public string Mac { get; set; } = string.Empty;

        [JsonPropertyName("region_id")]
        public int RegionId { get; set; }
    }
}
