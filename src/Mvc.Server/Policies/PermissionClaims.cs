using System.Collections.Generic;
using System.Linq;
using Mvc.Server.Core;

namespace Mvc.Server.Policies
{
    public static class PermissionClaims
    {
        public static IEnumerable<string> GetAll()
        {
            var type = typeof(Permissions);
            return type.GetFields().Select(permissionClaim => permissionClaim.GetValue(null).ToString()).ToList();
        }

        public static IEnumerable<string> GetAdminClaims()
        {
            return GetAll();
        }

        public static IEnumerable<string> GetAppUserClaims()
        {
            return GetAll().Where(x => x == Permissions.MessageUserPermissionClaim).ToList();
        }
    }
}