using Gateway.Models;
using Gateway.Repositories;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gateway.Mutations
{
    public class AccountMutation : ObjectGraphType<object>
    {
        public AccountMutation(IRepository repository)
        {

            this.FieldAsync<AccountType, Account>(
                "create",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AccountInputType>>()
                    {
                        Name = "account"
                    }),
                resolve: async context =>
                {
                    var account = context.GetArgument<Account>("account");
                    return await repository.AddOneAsync(account, context.CancellationToken);
                });

            this.FieldAsync<AccountType>(
                "update",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    },
                    new QueryArgument<NonNullGraphType<AccountInputType>>()
                    {
                        Name = "account"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    var account = context.GetArgument<Account>("account");
                    return await repository.UpdateOneAsync<Account>(x => x.Id == id, new BsonDocument {{ "$set", account.ToBsonDocument()}}, cancellationToken: context.CancellationToken);
                });

            this.FieldAsync<StringGraphType>(
                "delete",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    await repository.DeleteAsync<Account>(x => x.Id == id, context.CancellationToken);
                    return id;
                });
        }
    }
}