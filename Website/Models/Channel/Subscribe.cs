using System.Text.Json;
using System.Text.Json.Serialization;

namespace Website.Models.Channel
{
    public class Subscribe : BaseEvent
    {
        public Subscribe(JsonElement rootElement) : base(rootElement)
        {
            if(rootElement.TryGetProperty("tier", out var t))
            {
                Tier = t.GetString();
            }

            if(rootElement.TryGetProperty("is_gift", out var ig))
            {
                IsGift = ig.GetBoolean();
            }
        }

        [JsonPropertyName("tier")]
        public string Tier { get; set; }

        [JsonPropertyName("is_gift")]
        public bool IsGift { get; set; }
    }
}
