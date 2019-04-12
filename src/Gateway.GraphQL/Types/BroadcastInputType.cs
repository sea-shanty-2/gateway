using Gateway.Models;
using GraphQL.Types;

namespace Gateway.GraphQL.Types
{
    public class BroadcastInputType : InputObjectGraphType<Broadcast>
    {
        public BroadcastInputType()
        {
            Name = "BroadcastInput";
            Field(x => x.Categories);
            Field(x => x.Location, type: typeof(LocationInputType));
        }
    }
}