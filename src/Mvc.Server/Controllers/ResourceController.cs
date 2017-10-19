using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Server.Infrastructure.Mvc;
using MvcServer.Entities;
using System.Threading.Tasks;

namespace Mvc.Server.Controllers
{
    [Route("api")]
    public class ResourceController : BaseController
    {
        public ResourceController(UserManager<ApplicationUser> userManager) : base(userManager)
        {
        }

        [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("messageadmin")]
        public async Task<IActionResult> GetMessageAdmin()
        {
            var user = await CurrentUser();
            return Content($"{user.UserName} has been successfully authenticated.");
        }

        [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme, Roles = "User, Admin")]
        [HttpGet("messageuser")]
        public async Task<IActionResult> GetMessageUser()
        {
            var user = await CurrentUser();
            return Content($"{user.UserName} has been successfully authenticated.");
        }
    }
}