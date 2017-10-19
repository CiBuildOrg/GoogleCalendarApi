using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.DataObjects.Configuration
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class AppOptions
    {
        public Application Application { get; set; }
        public AuthSettings AuthenticationSettings { get; set; }
    }
}
