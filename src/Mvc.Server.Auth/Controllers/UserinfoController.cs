using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Server.Infrastructure.Mvc;
using MvcServer.Entities;
using Newtonsoft.Json.Linq;
using OpenIddict.Core;
using Mvc.Server.Infrastructure.Security;
using System.Collections.Generic;
using Mvc.Server.Core;

namespace Mvc.Server.Auth.Controllers
{
    [Route("api")]
    public class UserinfoController : BaseController
    {
        public UserinfoController(UserManager<ApplicationUser> userManager) : base(userManager)
        {
        }

        //
        // GET: /api/userinfo
        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
        [HttpGet("userinfo"), Produces("application/json")]
        public async Task<IActionResult> Userinfo()
        {
            var user = await CurrentUser();
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The user profile is no longer available."
                });
            }

            var claims = new JObject
            {
                [OpenIdConnectConstants.Claims.Subject] = await UserManager.GetUserIdAsync(user)
            };

            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.

            if (User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Email))
            {
                claims[OpenIdConnectConstants.Claims.Email] = await UserManager.GetEmailAsync(user);
                claims[OpenIdConnectConstants.Claims.EmailVerified] = await UserManager.IsEmailConfirmedAsync(user);
            }

            if (User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Phone))
            {
                claims[OpenIdConnectConstants.Claims.PhoneNumber] = await UserManager.GetPhoneNumberAsync(user);
                claims[OpenIdConnectConstants.Claims.PhoneNumberVerified] = await UserManager.IsPhoneNumberConfirmedAsync(user);
            }

            if (User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIddictConstants.Scopes.Roles))
            {
                List<string> rolesAndClaims = new List<string>();
                rolesAndClaims.AddRange(await UserManager.GetRolesAsync(user));

                foreach(var userClaim in PermissionClaims.GetAll())
                {
                    if(User.HasClaim(ApplicationConstants.PermissionClaimName, userClaim))
                    {
                        rolesAndClaims.Add(userClaim);
                    }
                }

                claims[OpenIddictConstants.Claims.Roles] = JArray.FromObject(rolesAndClaims);
            }

            foreach(var customClaim in PermissionClaims.GetAll())
            {
                //if(User.HasClaim(x => x.Subject.Name == customClaim))
                //{

                //}
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Json(claims);
        }
    }
}
