using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mvc.Server.Core;

namespace Mvc.Server.Auth.Controllers
{
    public class HomeController : Controller
    {
        //[Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme, Policy = Permissions.MessageAdminPermissionClaim)]
        [Authorize(Policy = Permissions.MessageAdminPermissionClaim)]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
