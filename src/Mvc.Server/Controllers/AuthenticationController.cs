using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Server.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet("~/signin"), AllowAnonymous]   
        public ActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "/",
                AllowRefresh = true,
            }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("~/logout"), HttpPost("~/logout"), AllowAnonymous]
        public ActionResult SignOut()
        {
            // is redirected from the identity provider after a successful authorization flow and
            // to redirect the user agent to the identity provider to sign out.
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}