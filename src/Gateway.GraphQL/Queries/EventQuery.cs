using Gateway.GraphQL.Extensions;
using Gateway.GraphQL.Types;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;
using MongoDB.Driver;

namespace Gateway.GraphQL.Queries
{
    public class EventQuery : ObjectGraphType<object>
    {
        public EventQuery(IRepository<Broadcast> repository)
        {
            Connection<BroadcastType>()
                .Name("page")
                .Description("Gets pages of broadcasts for the event.")
                .Bidirectional()
                .ResolveAsync(async context =>
                {
                    var entities = await repository.FindRangeAsync(_ => true);
                    return entities.ToConnection(context);
                });
        }
    }
}