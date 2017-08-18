using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mvc.Server.Filters;

namespace Mvc.Server.Helpers
{
    public class ExampleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        public ExampleMiddleware(RequestDelegate next, ILogger<CustomExceptionFilterAttribute> loggerFactory)
        {
            _next = next;
            _logger = loggerFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation("example middleware entry : " + context.Request.Path);

            await _next.Invoke(context);

            _logger.LogInformation("example middleware exit.");
        }
    }
}
