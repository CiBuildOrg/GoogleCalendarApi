using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Server.Models;

namespace Mvc.Server.Controllers
{
    [Route("api")]
    public class ResourceController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResourceController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager; 
        }

        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
        [HttpGet("message")]
        public async Task<IActionResult> GetMessage()
        {
            var userId = User.FindFirst(OpenIdConnectConstants.Claims.Subject).Value;
            var user = (await _userManager.GetUsersForClaimAsync(new Claim(OpenIdConnectConstants.Claims.Subject, userId))).SingleOrDefault();
            if (user == null)
            {
               throw new BadRequestException("No user found");
            }

            return Content($"{user.UserName} has been successfully authenticated.");
        }
    }

    [Serializable]
    public class BadRequestException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public BadRequestException()
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }

        public BadRequestException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BadRequestException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}