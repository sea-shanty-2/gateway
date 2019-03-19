using GraphQL.Types;

namespace Gateway.Queries
{
    public class StreamQuery : ObjectGraphType
    {
        public StreamQuery()
        {
            Field<BooleanGraphType>(
                "profile",
                resolve: context => {
                    return true;
                }
            );
        }
    }
}