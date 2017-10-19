namespace Mvc.Server.Core
{
    public static class ApplicationConstants
    {
        public const string AllowedUsernameCharacters = "abcdefghijklmnopqrstuvxyz1234567890!@#$%^&*()_+<>:|";
        public const string UserIdClaim = "useridclaim";
        public const string SecureSectionConfigurationPath = "SecureHeadersMiddlewareConfiguration";

        // cookie studd
        public const string CookieLoginPath = "/signin";
        public const string CookieLogoutPath = "/logout";

    }
}
