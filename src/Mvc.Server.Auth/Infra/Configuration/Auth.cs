using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.Auth.Infra.Configuration
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Auth
    {
        public bool UseHttps { get; set; }

        public bool AllowPasswordFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }

        public bool EnableAuthorize { get; set; }
        public bool EnableLogout { get; set; }
        public bool EnableToken { get; set; }
        public bool EnableUserInfo { get; set; }
        public bool EnableIntrospection { get; set; }

        public string AuthorizeEndpoint { get; set; }
        public string LogoutEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string UserInfoEndpoint { get; set; }
        public string IntrospectionEndpoint { get; set; }
    }
}