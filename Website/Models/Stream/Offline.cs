using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Website.Models.Stream
{
    public class Offline : BaseEvent
    {
        public Offline(JsonElement rootElement) : base(rootElement)
        {
        }
    }
}
