using System.Text.Json.Serialization;

namespace SEP6_AzureFunctions.Models
{
    public class Rating
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userid")]
        public string UserId { get; set; }

        [JsonPropertyName("movieid")]
        public string MovieId { get; set; }

        [JsonPropertyName("rating")]
        public int UserRating { get; set; }
    }
}
