using Microsoft.AspNetCore.Authorization;

namespace Mvc.Server.Policies
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission;

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}