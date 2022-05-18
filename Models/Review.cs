using System.Text.Json.Serialization;

namespace SEP6_AzureFunctions.Models
{
    public class Review
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userid")]
        public string UserId { get; set; }

        [JsonPropertyName("productionid")]
        public string ProductionId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("review")]
        public string UserReview { get; set; }

    }
}
