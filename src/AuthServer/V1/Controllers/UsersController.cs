using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OpenIddict.Core;
using Utils.Authorization;

namespace AuthServer.V1.Controllers
{
    [ApiController, ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;

        public UsersController(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        [Authorize(Roles = AppConstants.Roles.Admin)]
        [HttpGet("{id}"), Produces("application/json")]
        public async Task<IActionResult> GetUserInfo([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var claims = new JObject
            {
                [OpenIdConnectConstants.Claims.Subject] = await userManager.GetUserIdAsync(user),
                [OpenIdConnectConstants.Claims.Email] = await userManager.GetEmailAsync(user),
                [OpenIdConnectConstants.Claims.EmailVerified] = await userManager.IsEmailConfirmedAsync(user),
                [OpenIdConnectConstants.Claims.PhoneNumber] = await userManager.GetPhoneNumberAsync(user),
                [OpenIdConnectConstants.Claims.PhoneNumberVerified] = await userManager.IsPhoneNumberConfirmedAsync(user),
                [OpenIddictConstants.Scopes.Roles] = JArray.FromObject(await userManager.GetRolesAsync(user))
            };

            return Json(claims);
        }

        /// <summary>
        /// Returns basic claims for current user.
        /// </summary>
        [Authorize(OpenIdConnectConstants.Scopes.OpenId)]
        [HttpGet("me"), Produces("application/json")]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The user profile is no longer available."
                });
            }

            var isCookie = User.IsCookieAuthenticated();

            var claims = new JObject
            {
                [OpenIdConnectConstants.Claims.Subject] = await userManager.GetUserIdAsync(user)
            };

            if (isCookie || User.HasScope(OpenIdConnectConstants.Scopes.Email))
            {
                claims[OpenIdConnectConstants.Claims.Email] = await userManager.GetEmailAsync(user);
                claims[OpenIdConnectConstants.Claims.EmailVerified] = await userManager.IsEmailConfirmedAsync(user);
            }

            if (isCookie || User.HasScope(OpenIdConnectConstants.Scopes.Phone))
            {
                claims[OpenIdConnectConstants.Claims.PhoneNumber] = await userManager.GetPhoneNumberAsync(user);
                claims[OpenIdConnectConstants.Claims.PhoneNumberVerified] = await userManager.IsPhoneNumberConfirmedAsync(user);
            }

            if (isCookie || User.HasScope(OpenIddictConstants.Scopes.Roles))
            {
                claims[OpenIddictConstants.Scopes.Roles] = JArray.FromObject(await userManager.GetRolesAsync(user));
            }

            return Json(claims);
        }
    }
}
