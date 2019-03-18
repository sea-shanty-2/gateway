using Gateway.Data;
using Gateway.Models;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gateway.Query
{
    public class EnvueQuery : ObjectGraphType
    {
        public EnvueQuery(EnvueData data)
        {

            FieldAsync<UserType>(
                "user",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "The ID of the user." }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    return await data.Users.Find(x => x.Id == id).FirstOrDefaultAsync();
                });

            FieldAsync<ListGraphType<UserType>>(
                "users",
                resolve: async context =>
                {
                    return await data.Users.Find(_ => true).ToListAsync();
                    
                });
        }
    }
}