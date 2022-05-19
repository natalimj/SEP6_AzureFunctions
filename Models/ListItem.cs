using Newtonsoft.Json;

namespace SEP6_AzureFunctions.Models
{
    public class ListItem
    {
        [JsonProperty("productionid")]
        public string ProductionId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
