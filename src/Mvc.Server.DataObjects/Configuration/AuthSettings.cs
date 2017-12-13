using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.DataObjects.Configuration
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class AuthSettings
    {
        public string AuthorityUrl { get; set; }

        public bool UseApi { get; set; }
        public bool UseWeb { get; set; }
        public ApiSettings ApiSettings { get; set; }
        public WebSettings WebSettings { get; set; }
    }
}