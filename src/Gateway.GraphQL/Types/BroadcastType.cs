using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;

namespace Gateway.GraphQL.Types
{
    public class BroadcastType : ObjectGraphType<Broadcast>
    {
        public BroadcastType()
        {
            Name = "Broadcast";
            Field(x => x.Id);
            Field(x => x.Location, type: typeof(LocationType));
            Field(x => x.Activity, type: typeof(DateTimeGraphType));
        }
    }
    
}