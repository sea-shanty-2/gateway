using GraphQL;
using GraphQL.Types;

namespace Gateway
{
    public class Schema : GraphQL.Types.Schema
    {
        public Schema(IDependencyResolver resolver): base(resolver)
        {
            Query = resolver.Resolve<Query>();
            Mutation = resolver.Resolve<Mutation>();
        }
    }
}

