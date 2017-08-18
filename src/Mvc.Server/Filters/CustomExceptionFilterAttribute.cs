using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Mvc.Server.Controllers;

namespace Mvc.Server.Filters
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        public CustomExceptionFilterAttribute(ILogger<CustomExceptionFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            // request the IHostingEnvironment to enable conditional behavior such as returning all stacktrace on clients.  
            //if (_hostingEnvironment.IsDevelopment())
            //{
            //    return;
            //}

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

    public class ValidateModelFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Allow partial update
            if (!context.ModelState.IsValid && (context.HttpContext.Request.Method == "PATCH" || context.HttpContext.Request.Method == "PUT"))
            {
                // get the errors which only have 'required type' error
                var modelStateErrors = context.ModelState.Where(model =>
                {
                    // ignore only if required error is present for the property
                    if (model.Value.Errors.Count == 1)
                    {
                        // improve code to remove check on hard coded string - "required"
                        // assuming required validation error message contains word "required"
                        return model.Value.Errors.FirstOrDefault().ErrorMessage.Contains("required");
                    }
                    return false;
                });
                // remove 'required type' errors from the ModelState
                foreach (var errorModel in modelStateErrors)
                {
                    context.ModelState.Remove(errorModel.Key);
                }

            }
            // Return validation error response
            if (!context.ModelState.IsValid)
            {
                var modelErrors = new Dictionary<string, Object>();
                modelErrors["message"] = "The request has validation errors.";
                modelErrors["errors"] = new SerializableError(context.ModelState);
                context.Result = new BadRequestObjectResult(modelErrors);
            }
        }
    }
}
