using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.Auth.Infra.Configuration
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class AppOptions
    {
        public Application Application { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public TokenGeneration TokenGeneration { get; set; }
        public Auth Auth { get; set; }
    }
}
