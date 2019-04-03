using System.Threading.Tasks;
using GraphQL.Authorization;

namespace Gateway.Requirements
{
    public class AuthenticatedUserRequirement : IAuthorizationRequirement
    {
        public Task Authorize(AuthorizationContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
                context.ReportError("");
            return Task.CompletedTask;
        }
    }
}