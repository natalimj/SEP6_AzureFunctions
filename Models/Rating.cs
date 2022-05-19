using Newtonsoft.Json;

namespace SEP6_AzureFunctions.Models
{
    public class Rating
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("productionid")]
        public string ProductionId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("rating")]
        public int UserRating { get; set; }
    }
}
