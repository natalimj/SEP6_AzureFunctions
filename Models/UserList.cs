using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace SEP6_AzureFunctions.Models
{
    public class UserList
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userid")]
        public string UserId { get; set; }

        [JsonPropertyName("listname")]
        public string ListName { get; set; }

        [JsonPropertyName("movies")]
        public List<string> Movies { get; set; }

    }
}
