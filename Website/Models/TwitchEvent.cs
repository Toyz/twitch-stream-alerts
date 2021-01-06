using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Website.Models
{
    public class TwitchEvent
    {
        // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
        public class Condition
        {
            [JsonPropertyName("broadcaster_user_id")]
            public string BroadcasterUserId { get; set; }
        }

        public class Transport
        {
            [JsonPropertyName("method")]
            public string Method { get; set; }

            [JsonPropertyName("callback")]
            public string Callback { get; set; }
        }

        public class Subscription
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("version")]
            public string Version { get; set; }

            [JsonPropertyName("condition")]
            public Condition Condition { get; set; }

            [JsonPropertyName("transport")]
            public Transport Transport { get; set; }

            [JsonPropertyName("created_at")]
            public DateTime CreatedAt { get; set; }
        }

        public class Root
        {
            [JsonPropertyName("subscription")]
            public Subscription Subscription { get; set; }

            [JsonPropertyName("event")]
            public JsonDocument Event { get; set; }
        }


    }
}
