using Gateway.Models;
using GraphQL.Types;

namespace Gateway.GraphQL.Types
{
    public class BroadcastInputType : InputObjectGraphType<Broadcast>
    {
        public BroadcastInputType()
        {
            Field(x => x.Title);
            Field(x => x.Tag);
            Field(x => x.Location, type: typeof(LocationInputType));
        }
    }
}