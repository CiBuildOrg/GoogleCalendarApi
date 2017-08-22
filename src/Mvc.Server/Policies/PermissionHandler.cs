using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Mvc.Server.Policies
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User.IsInRole("Admin"))
            {
                System.Console.WriteLine("Admin user, don't verify");
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            // All the role permission claims are present in the jwt scope claim 
            if (context.User.HasClaim(c => c.Type == "scope" && c.Value.Contains(requirement.Permission))
                || context.User.HasClaim(x => x.Type == "permission" && x.Value.Contains(requirement.Permission)))
            {
                System.Console.WriteLine("User is not admin but has required permission: " + requirement.Permission);
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            System.Console.WriteLine("User is forbidden");
            context.Fail();
            return Task.CompletedTask;
        }
    }
}