using System;
using System.Collections.Generic;
using System.Linq;

namespace Mvc.Server.Policies
{
    public static class PermissionClaims
    {



        public const string ReadUser = "user:read";
        public const string ReadUsers = "user:readAll";
        public const string CreateUser = "user:create";
        public const string UpdateUser = "user:update";
        public const string DeleteUser = "user:delete";

        public const string ReadRole = "role:read";
        public const string ReadRoles = "role:readAll";
        public const string CreateRole = "role:create";
        public const string UpdateRole = "role:update";
        public const string DeleteRole = "role:delete";

        public const string MessageAdmin = "message:admin";
        public const string MessageUser = "message:user";

        public static List<string> GetAll()
        {
            var type = typeof(PermissionClaims);
            return type.GetFields().Select(permissionClaim => permissionClaim.GetValue(null).ToString()).ToList();
        }

        public static List<string> GetAdminClaims()
        {
            return GetAll();
        }

        public static List<string> GetAppUserClaims()
        {
            return GetAll().Where(x => x == MessageAdmin).ToList();
        }
    }
}