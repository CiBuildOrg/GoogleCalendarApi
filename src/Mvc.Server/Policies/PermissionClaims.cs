using System.Collections.Generic;
using System.Linq;

namespace Mvc.Server.Policies
{
    public static class PermissionClaims
    {
        public const string MessageAdmin = "message:admin";
        public const string MessageUser = "message:user";

        private static List<string> GetAll()
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
            return GetAll().Where(x => x == MessageUser).ToList();
        }
    }
}