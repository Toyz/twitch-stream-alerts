using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Website.Models.Stream
{
    public enum StreamType
    {
        live,
        playlist,
        watch_party,
        premiere,
        rerun
    }

    public class Online : BaseEvent
    {
        public Online(JsonElement rootElement) : base(rootElement)
        {
            ID = rootElement.GetProperty("id").GetString();
            StreamType = Enum.Parse<StreamType>(rootElement.GetProperty("type").GetString(), true);
        }

        public string ID { get; set; }
        public StreamType StreamType { get; set; }
    }
}
