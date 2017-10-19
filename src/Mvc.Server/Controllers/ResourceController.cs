using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Server.Infrastructure.Mvc;
using MvcServer.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

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

            return Content($"You have authorized access to resources belonging to {identity.Name} on ResourceServer01.");
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

            return Content($"You have authorized access to resources belonging to {identity.Name} on ResourceServer01.");
        }
    }
}