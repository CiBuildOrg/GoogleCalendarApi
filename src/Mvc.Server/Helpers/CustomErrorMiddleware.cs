using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mvc.Server.Filters;
using Newtonsoft.Json;

namespace Mvc.Server.Helpers
{
    public class CustomErrorMiddleware
    {
        private const string UnauthorizedMessage = "You are not authorized to access this endpoint";
        private const string ForbiddenMessage = "You are forbidden to access this endpoint";

        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        public CustomErrorMiddleware(RequestDelegate next, ILogger<CustomExceptionFilterAttribute> loggerFactory)
        {
            _next = next;
            _logger = loggerFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation("example middleware entry : " + context.Request.Path);
            var originBody = context.Response.Body;

            var newBody = new MemoryStream();
            context.Response.Body = newBody;
            await _next(context);
            newBody.Seek(0, SeekOrigin.Begin);
            var json = new StreamReader(newBody).ReadToEnd();

            context.Response.Body = originBody;

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
                {
                    var newJson = JsonConvert.SerializeObject(new CustomResponseError
                    {
                        Success = false,
                        Message = ForbiddenMessage
                    }, _jsonSettings);

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(newJson);
                }
                else if (context.Response.StatusCode == (int) HttpStatusCode.Unauthorized)
                {
                    var newJson = JsonConvert.SerializeObject(new CustomResponseError
                    {
                        Success = false,
                        Message = UnauthorizedMessage
                    }, _jsonSettings);

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(newJson);
                }
                else await context.Response.WriteAsync(json);
            }
            else
            await context.Response.WriteAsync(json);

            _logger.LogInformation("example middleware exit.");
        }
    }
}
