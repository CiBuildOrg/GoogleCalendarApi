using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.Options
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ConnectionStrings
    {
        public string SqlServerProvider { get; set; }
    }
}