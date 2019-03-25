namespace Gateway.Schemas
{
    using Gateway.Mutations;
    using Gateway.Queries;
    using GraphQL;
    using GraphQL.Types;

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
            /*this.Subscription = subscription; */
        }
    }
}
