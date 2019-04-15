using Gateway.Models;
using GraphQL.Types;

namespace Gateway.GraphQL.Types
{
    public class BroadcastInputType : InputObjectGraphType<Broadcast>
    {
        public BroadcastInputType()
        {
            Field(x => x.Categories);
            Field(x => x.Location, type: typeof(LocationInputType));
        }
    }
}