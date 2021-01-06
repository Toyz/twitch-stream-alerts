using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Website.Models.Channel
{
    public class Follow : BaseEvent
    {
        public Follow(JsonElement rootElement) : base(rootElement)
        {
        }
    }
}
