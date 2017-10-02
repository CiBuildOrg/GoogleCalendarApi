using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Mvc.Server.Exceptions;

namespace Mvc.Server.Infrastructure.Attributes
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        public CustomExceptionFilterAttribute(ILogger<CustomExceptionFilterAttribute> logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public override void OnException(ExceptionContext context)
        {
            // request the IHostingEnvironment to enable conditional behavior such as returning all stacktrace on clients.  
            if (_hostingEnvironment.IsDevelopment())
            {
                return;
            }

            const string message = "Oops! Something is broken, we are looking into it";
            _logger.LogError(0, context.Exception, message);
            if (context.Exception is BadRequestException)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            else context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result = new JsonResult(new
            {
                success = false,
                message
            });
        }
    }
}
