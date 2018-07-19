using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AuthServer.Models;
using AuthServer.V1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OpenIddict.Abstractions;
using Utils;
using Utils.Authorization;

namespace AuthServer.V1.Controllers
{
    [ApiController, ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        [Authorize(AppConstants.Scopes.UsersRead, Roles = AppConstants.Roles.Administrator)]
        [HttpGet, Produces("application/json")]
        public async Task<ActionResult<List<UserResult>>> GetUsers()
        {
            var result = new List<UserResult>();
            var users = userManager.Users;
            foreach (var user in users)
            {
                result.Add(new UserResult
                {
                    Subject = user.Id,
                    Email = user.Email,
                    Roles = (await userManager.GetRolesAsync(user)).ToList()
                });
            }

            return result;
        }

        [Authorize(AppConstants.Scopes.UsersRead, Roles = AppConstants.Roles.Administrator)]
        [HttpGet("{id}"), Produces("application/json")]
        public async Task<ActionResult<UserResult>> GetUserInfo([FromRoute] string id)
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

            return new UserResult
            {
                Subject = await userManager.GetUserIdAsync(user),
                Email = await userManager.GetEmailAsync(user),
                Roles = (await userManager.GetRolesAsync(user)).ToList()
            };
        }

        /// <summary>
        /// Returns basic claims for current user.
        /// </summary>
        [Authorize(AppConstants.Scopes.OpenId)]
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
