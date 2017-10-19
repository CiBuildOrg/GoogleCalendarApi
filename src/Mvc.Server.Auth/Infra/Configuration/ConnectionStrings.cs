using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.Auth.Infra.Configuration
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ConnectionStrings
    {
        public string SqlServerProvider { get; set; }
    }
}