namespace Mvc.Server.Core
{
    public static class ApplicationConstants
    {
        public const string AllowedUsernameCharacters = "abcdefghijklmnopqrstuvxyz1234567890!@#$%^&*()_+<>:|";
        public const string UserIdClaim = "useridclaim";
        public const string PermissionClaimName = "permission";


        public const string MessageAdminPermissionClaim = "message:admin";
        public const string MessageUserPermissionClaim = "message:user";
    }
}
