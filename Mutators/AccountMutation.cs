using Gateway.Models;
using GraphQL.Types;
using MongoDB.Driver;

namespace Gateway.Mutators
{
    public class AccountMutation : ObjectGraphType
    {
        public AccountMutation(Data data)
        {
            Field<BooleanGraphType>(
                "create",
                resolve: context =>
                {
                    data.GetCollection<Account>().InsertOne(new Account());
                    return true;
                }
            );

            FieldAsync<BooleanGraphType>(
                "delete",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "The ID of the account." }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    return (await data.GetCollection<Account>().DeleteOneAsync(x => x.Id == id)).IsAcknowledged;
                }
            );
        }

    }
}