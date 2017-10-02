using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Mvc.Server.Infrastructure.Utils.Middlewares;

namespace Mvc.Server.Infrastructure.Extensions
{
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static class CustomMiddlewareExtensions
    {
        public static IApplicationBuilder UseExampleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomErrorMiddleware>();
        }
    }
}