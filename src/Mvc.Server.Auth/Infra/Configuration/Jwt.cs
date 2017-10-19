using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.Auth.Infra.Configuration
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Jwt
    {
        public string Audience { get; set; }
        public string Authority { get; set; }
        public string SecretKey { get; set; }
        public int AccessTokenLifetime { get; set; }
        public int IdentityTokenLifetime { get; set; }
        public int RefreshTokenLifetime { get; set; }
    }
}