using Gateway.GraphQL.Types;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gateway.GraphQL.Mutations
{
    public class AccountMutation : ObjectGraphType<object>
    {
        public AccountMutation(IRepository<Account> repository)
        {

            this.FieldAsync<AccountType>(
                "create",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AccountInputType>>()
                    {
                        Name = "account"
                    }),
                resolve: async context =>
                {
                    var account = context.GetArgument<Account>("account");
                    return await repository.AddAsync(account, context.CancellationToken);
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
                    return await repository.UpdateAsync(x => x.Id == id, account, context.CancellationToken);
                });

            this.FieldAsync<IdGraphType>(
                "delete",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    await repository.RemoveAsync(x => x.Id == id, context.CancellationToken);
                    return id;
                });
        }
    }
}