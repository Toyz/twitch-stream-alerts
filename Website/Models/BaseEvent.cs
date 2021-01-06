using System.Text.Json;
using System.Text.Json.Serialization;

namespace Website.Models
{
    public class BaseEvent
    {
        public BaseEvent(JsonElement rootElement)
        {
            if(rootElement.TryGetProperty("user_id", out var uid))
            {
                UserId = uid.GetString();
            }

            if (rootElement.TryGetProperty("user_name", out var uname))
            {
                UserName = uname.GetString();
            }

            if(rootElement.TryGetProperty("broadcaster_user_id", out var bid))
            {
                BroadcasterUserId = bid.GetString();
            }

            if(rootElement.TryGetProperty("broadcaster_user_name", out var bun))
            {
                BroadcasterUserName = bun.GetString();
            }

            if(rootElement.TryGetProperty("broadcast_user_id", out var bid2)) {
                BroadcasterUserId = bid2.GetString();
            }

            if (rootElement.TryGetProperty("broadcast_user_name", out var bun2))
            {
                BroadcasterUserName = bun2.GetString();
            }
        }
        
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; }

        [JsonPropertyName("broadcaster_user_name")]
        public string BroadcasterUserName { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}