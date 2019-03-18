using Gateway.Data;
using Gateway.Models;
using Gateway.Types;
using GraphQL.Types;

namespace Gateway.Mutation
{
    public class EnvueMutation : ObjectGraphType
    {
        public EnvueMutation(EnvueData data)
        {
            FieldAsync<BooleanGraphType>(
                "createUser",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<UserInputType>> { Name = "user" }
                ),
                resolve: async context =>
                {
                    var user = context.GetArgument<User>("user");
                    await data.Users.InsertOneAsync(user);
                    return true;
                }
            );
        }
    }
}