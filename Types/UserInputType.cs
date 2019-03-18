using GraphQL.Types;

namespace Gateway.Types
{
    public class UserInputType : InputObjectGraphType
    {
        public UserInputType()
        {
            Name = "UserInput";

            Field<NonNullGraphType<StringGraphType>>("name");
        }
    }
}