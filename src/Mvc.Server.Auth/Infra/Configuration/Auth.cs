using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.Auth.Infra.Configuration
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Auth
    {
        public string AuthorizeEndpoint { get; set; }
        public string LogoutEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string UserInfoEndpoint { get; set; }
        public string IntrospectionEndpoint { get; set; }
    }
}