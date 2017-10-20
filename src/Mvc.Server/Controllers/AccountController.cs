using Microsoft.AspNetCore.Mvc;

namespace Mvc.Server.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet, Route("~/account/denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}