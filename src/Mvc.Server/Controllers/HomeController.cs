using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mvc.Server.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Mvc.Server.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var token = await HttpContext.GetTokenAsync(OAuthValidationDefaults.AuthenticationScheme, "access_token");

            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
