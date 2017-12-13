using System.Diagnostics.CodeAnalysis;

namespace Mvc.Server.DataObjects.Configuration
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Application
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
}