using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Website.Models
{
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

        [JsonPropertyName("secret")]
        public string Secret { get; set; }
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

    public class TwitchEvent
    {
        [JsonPropertyName("subscription")]
        public Subscription Subscription { get; set; }

        [JsonPropertyName("event")]
        public JsonDocument Event { get; set; }

        [JsonPropertyName("challenge")]
        public string Challenge { get; set; }

        public BaseEvent EventObject => Subscription.Type switch
        {
            "stream.online" => new Stream.Online(Event.RootElement),
            "stream.offline" => new Stream.Offline(Event.RootElement),
            _ => throw new Exception($"{Subscription.Type} is an unknown event type")
        };
    }
}
