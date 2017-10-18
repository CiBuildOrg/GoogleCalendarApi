using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Server.Core;
using Mvc.Server.Infrastructure.Mvc;
using MvcServer.Entities;
using AspNet.Security.OAuth.Validation;

namespace Mvc.Server.Auth.Controllers
{
    [Route("api")]
    public class ResourceController : BaseController
    {
        public ResourceController(UserManager<ApplicationUser> userManager) : base(userManager)
        {
        }

        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("messageadmin")]
        public async Task<IActionResult> GetMessageAdmin()
        {
            var user = await CurrentUser();
            return Content($"{user.UserName} has been successfully authenticated.");
        }

        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme, Roles = "Admin,User")]
        [HttpGet("messageuser")]
        public async Task<IActionResult> GetMessageUser()
        {
            var user = await CurrentUser();
            return Content($"{user.UserName} has been successfully authenticated.");
        }
    }
}