using Microsoft.AspNetCore.Authorization;

namespace Mvc.Server.Infrastructure.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public readonly string Permission;

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}