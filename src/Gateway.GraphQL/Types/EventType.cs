using Gateway.Models;
using GraphQL.Types;

namespace Gateway.GraphQL.Types
{
    public class EventType : ObjectGraphType<Event>
    {
        public EventType() {
            Field(x => x.Broadcasts, type: typeof(ListGraphType<BroadcastType>));
            Field(x => x.Recommended, type: typeof(BroadcastType));
        }
    }
}