using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Mvc.Server.Policies
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User.IsInRole("Admin"))
            {
                // user is an admin so they're automatically allowed in
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // All the role permission claims are present in the jwt scope claim 
            if (context.User.HasClaim(c => c.Type == "scope" && c.Value.Contains(requirement.Permission))
                || context.User.HasClaim(x => x.Type == "permission" && x.Value.Contains(requirement.Permission)))
            {
                // user is allowed 
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // user is forbidden
            context.Fail();
            return Task.CompletedTask;
        }
    }
}