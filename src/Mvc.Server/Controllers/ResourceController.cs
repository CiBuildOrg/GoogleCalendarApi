using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mvc.Server.Infrastructure.Mvc;
using System.Security.Claims;

namespace Mvc.Server.Controllers
{
    [Route("api")]
    public class ResourceController : BaseController
    {
        public ResourceController() 
        {
        }

        [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("messageadmin")]
        public IActionResult GetMessageAdmin()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return BadRequest();
            }

            return Content($"You have authorized access to resources belonging to {identity.Name} on the resource server.");
        }

        [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme, Roles = "User, Admin")]
        [HttpGet("messageuser")]
        public IActionResult GetMessageUser()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return BadRequest();
            }

            return Content($"You have authorized access to resources belonging to {identity.Name} on the resource server.");
        }
    }
}