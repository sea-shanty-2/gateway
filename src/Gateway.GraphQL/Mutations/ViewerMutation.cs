using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;

namespace Gateway.GraphQL.Mutations
{
    public class ViewerMutation : ObjectGraphType<object>
    {
        public ViewerMutation(IRepository<Viewer> repository) {
            
        }
}
}