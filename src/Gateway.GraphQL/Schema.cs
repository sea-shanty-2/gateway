using GraphQL;
using GraphQL.Types;

namespace Gateway.GraphQL
{
    public class MainSchema : Schema
    {
        public MainSchema(
            Query query,
            Mutation mutation,
            IDependencyResolver resolver)
            : base(resolver)
        {
            Query = query;
            Mutation = mutation;
        }
    }
}
