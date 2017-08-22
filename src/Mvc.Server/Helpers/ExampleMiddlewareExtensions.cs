using Microsoft.AspNetCore.Builder;

namespace Mvc.Server.Helpers
{
    public static class ExampleMiddlewareExtensions
    {
        public static IApplicationBuilder UseExampleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomErrorMiddleware>();
        }
    }
}