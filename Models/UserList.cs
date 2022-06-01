using System.Collections.Generic;
using Newtonsoft.Json;

namespace SEP6_AzureFunctions.Models
{
    public class UserList
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("listname")]
        public string ListName { get; set; }

        [JsonProperty("listItems")]
        public List<ListItem> ListItems { get; set; }

    }
}
