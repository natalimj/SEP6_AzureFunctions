using System.Text.Json.Serialization;

namespace SEP6_AzureFunctions.Models
{
    public class Review
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userid")]
        public string UserId { get; set; }

        [JsonPropertyName("movieid")]
        public string MovieId { get; set; }

        [JsonPropertyName("review")]
        public string UserReview { get; set; }

    }
}
