using Newtonsoft.Json;

namespace ProjectLauncherTemplate.Models
{
    public class GameVersion
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "0.0.0";

        [JsonProperty("buildUrl")]
        public string BuildUrl { get; set; } = string.Empty;

        [JsonProperty("notes")]
        public string Notes { get; set; } = string.Empty;
    }
}
