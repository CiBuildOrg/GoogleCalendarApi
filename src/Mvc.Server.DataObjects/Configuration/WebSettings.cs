using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.DataObjects.Configuration
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class WebSettings
    {
        public string Resource { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}