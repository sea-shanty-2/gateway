using Gateway.Models;
using GraphQL.Types;

namespace Gateway.Types
{
    public class UserType : ObjectGraphType<User>
    {
        public UserType()
        {
            Name = "User";

            Field(x => x.Id).Description("The id of the user");
            Field(x => x.Name).Description("The name of the user");
        }
    }
}