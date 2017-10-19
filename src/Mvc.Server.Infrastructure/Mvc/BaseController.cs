using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Server.Core;
using Mvc.Server.Exceptions;
using MvcServer.Entities;
using System;

namespace Mvc.Server.Infrastructure.Mvc
{
    public abstract class BaseController : Controller
    {
        protected UserManager<ApplicationUser> UserManager { get; }

        protected BaseController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        protected BaseController()
        {

        }

        private async Task<ApplicationUser> GetCurrentUser()
        {
            if (UserManager == null)
                throw new Exception("User manager not set");

            var userId = User.FindFirst(ApplicationConstants.UserIdClaim).Value;
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