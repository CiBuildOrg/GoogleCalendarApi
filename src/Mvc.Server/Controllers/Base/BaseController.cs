using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Server.Exceptions;
using Mvc.Server.Helpers;
using Mvc.Server.Models;

namespace Mvc.Server.Controllers.Base
{
    public abstract class BaseController : Controller
    {
        protected UserManager<ApplicationUser> UserManager { get; set; }

        protected BaseController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        private async Task<ApplicationUser> GetCurrentUser()
        {
            var userId = User.FindFirst(CustomClaims.UserIdClaim).Value;
            var user = (await UserManager.FindByIdAsync(userId));
            if (user == null)
            {
                throw new BadRequestException("No user found");
            }

            return user;
        }

        protected async Task<ApplicationUser> CurrentUser() => await GetCurrentUser();
    }
}