using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;

namespace Gateway.GraphQL.Types
{
    public class BroadcastType : ObjectGraphType<Broadcast>
    {
        public BroadcastType()
        {
            Field(x => x.Id);
            Field(x => x.Title);
            Field(x => x.Tag);
            Field(x => x.Location, type: typeof(LocationType));
        }
    }

    public class BroadcastCreateType : ObjectGraphType<Broadcast>
    {
        public BroadcastCreateType()
        {
            Field(x => x.Id);
            Field(x => x.Token);
        }
    }
    
    public class BroadcastUpdateType : ObjectGraphType<Broadcast>
    {
        public BroadcastUpdateType()
        {
            Field(x => x.Id);
            Field(x => x.Title);
        }
    }
}