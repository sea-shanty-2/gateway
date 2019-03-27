using Gateway.Models;
using Gateway.Repositories;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Driver;

namespace Gateway.Mutations
{
    public class AccountMutation : ObjectGraphType<object>
    {
        public AccountMutation(IRepository repository)
        {

            this.FieldAsync<AccountGraphType, Account>(
                "create",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<InputAccountGraphType>>()
                    {
                        Name = "account"
                    }),
                resolve: async context =>
                {
                    var account = context.GetArgument<Account>("account");
                    return await repository.AddAsync(account, context.CancellationToken);
                });

            this.FieldAsync<AccountGraphType>(
                "update",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    },
                    new QueryArgument<NonNullGraphType<InputAccountGraphType>>()
                    {
                        Name = "account"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    var account = context.GetArgument<Account>("account");
                    return await repository.UpdateAsync(x => x.Id == id, account, context.CancellationToken);
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