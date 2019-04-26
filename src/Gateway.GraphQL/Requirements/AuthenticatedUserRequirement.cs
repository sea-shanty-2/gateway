using System.Threading.Tasks;
using GraphQL.Authorization;

namespace Gateway.GraphQL.Requirements
{
    public class AuthenticatedUserRequirement : IAuthorizationRequirement
    {
        public Task Authorize(AuthorizationContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
                context.ReportError("Identity is unauthenticated");

            return Task.CompletedTask;
        }
    }
}