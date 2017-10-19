using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.Auth.Infra.Configuration
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class TokenGeneration
    {
        public bool IncludeUserIdClaim { get; set; }

        public List<string> Audiences { get; set; }
        public List<string> Resources { get; set; }
        public string AuthorityUrl { get; set; }

        public int AccessTokenLifetime { get; set; }
        public int IdentityTokenLifetime { get; set; }
        public int RefreshTokenLifetime { get; set; }
    }
}