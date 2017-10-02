using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;

namespace Mvc.Server.Helpers
{
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static class ExampleMiddlewareExtensions
    {
        public static IApplicationBuilder UseExampleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomErrorMiddleware>();
        }
    }
}