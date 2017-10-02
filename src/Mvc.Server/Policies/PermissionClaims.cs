using System.Collections.Generic;
using System.Linq;
using Mvc.Server.Core;

namespace Mvc.Server.Policies
{
    public static class PermissionClaims
    {
        private static IEnumerable<string> GetAll()
        {
            var type = typeof(PermissionClaims);
            return type.GetFields().Select(permissionClaim => permissionClaim.GetValue(null).ToString()).ToList();
        }

        public static IEnumerable<string> GetAdminClaims()
        {
            return GetAll();
        }

        public static IEnumerable<string> GetAppUserClaims()
        {
            return GetAll().Where(x => x == ApplicationConstants.MessageUserPermissionClaim).ToList();
        }
    }
}