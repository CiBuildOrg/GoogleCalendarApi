using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Server.Core;
using Mvc.Server.Infrastructure.Mvc;
using MvcServer.Entities;

namespace Mvc.Server.Controllers
{
    [Route("api")]
    public class ResourceController : BaseController
    {
        public ResourceController(UserManager<ApplicationUser> userManager) : base(userManager)
        {
        }

        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme, Policy = Permissions.MessageAdminPermissionClaim)]
        [HttpGet("messageadmin")]
        public async Task<IActionResult> GetMessageAdmin()
        {
            var user = await CurrentUser();
            return Content($"{user.UserName} has been successfully authenticated.");
        }

        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme, Policy = Permissions.MessageUserPermissionClaim)]
        [HttpGet("messageuser")]
        public async Task<IActionResult> GetMessageUser()
        {
            var user = await CurrentUser();
            return Content($"{user.UserName} has been successfully authenticated.");
        }
    }
}