using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Website.Models.Channel
{
    public class Update : BaseEvent
    {
        public Update(JsonElement rootElement) : base(rootElement)
        {
            Title = rootElement.GetProperty("title").GetString();
            Language = rootElement.GetProperty("language").GetString();
            CategoryId = rootElement.GetProperty("category_id").GetString();
            CategoryName = rootElement.GetProperty("category_name").GetString();
            IsMature = rootElement.GetProperty("is_mature").GetBoolean();
        }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("category_id")]
        public string CategoryId { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        [JsonPropertyName("is_mature")]
        public bool IsMature { get; set; }
    }
}
