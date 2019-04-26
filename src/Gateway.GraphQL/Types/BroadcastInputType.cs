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

    public class BroadcastUpdateInputType : InputObjectGraphType<Broadcast>
    {
        public BroadcastUpdateInputType()
        {
            Field(x => x.Location, type: typeof(LocationInputType), nullable: true);
            Field(x => x.Bitrate, nullable: true);
            Field(x => x.Stability, nullable: true);
            Field(x => x.Activity, type: typeof(DateTimeGraphType), nullable: true);
        }
    }
} 