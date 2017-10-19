using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.DataObjects.Configuration
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ApiSettings
    {
        public string Audience { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool RequireHttps { get; set; }
    }
}