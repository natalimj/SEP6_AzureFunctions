using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SEP6_AzureFunctions.Models
{
    public class ListItem
    {
        [JsonPropertyName("productionid")]
        public string ProductionId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
