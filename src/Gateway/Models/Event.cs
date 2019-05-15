using System.Collections.Generic;

namespace Gateway.Models
{
    public class Event
    {
        public IEnumerable<Broadcast> Broadcasts { get; set; }
        public Broadcast Recommended { get; set; }
    }
}