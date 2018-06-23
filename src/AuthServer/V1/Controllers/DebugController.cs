#if DEBUG

using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AuthServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AuthServer.V1.Controllers
{
    [ApiController, ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DebugController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public DebugController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [Authorize]
        [HttpGet("me"), Produces("application/json")]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The user profile is no longer available."
                });
            }

            var identities = new JArray();
            var result = new JObject
            {
                ["claims"] = new JArray(User.Claims.Select(c => new JArray(c.Type, c.Value))),
                ["tokens"] = new JObject
                {
                    ["Google"] = new JObject
                    {
                        ["access_token"] = await userManager.GetAuthenticationTokenAsync(currentUser, "Google", "access_token"),
                        ["id_token"] = await userManager.GetAuthenticationTokenAsync(currentUser, "Google", "refresh_token"),
                        ["refresh_token"] = await userManager.GetAuthenticationTokenAsync(currentUser, "Google", "id_token")
                    }
                },
                ["identities"] = identities
            };

            foreach (var claimsIdentity in User.Identities)
            {
                identities.Add(new JObject
                {
                    ["IsAuthenticated"] = claimsIdentity.IsAuthenticated,
                    ["AuthenticationType"] = claimsIdentity.AuthenticationType,
                    ["Label"] = claimsIdentity.Label,
                    ["Name"] = claimsIdentity.Name,
                    ["NameClaimType"] = claimsIdentity.NameClaimType,
                    ["RoleClaimType"] = claimsIdentity.RoleClaimType,
                    ["Claims"] = new JArray(claimsIdentity.Claims.Select(c => new JObject
                    {
                        ["Type"] = c.Type,
                        ["Value"] = c.Value,
                        ["ValueType"] = c.ValueType,
                        ["Issuer"] = c.Issuer,
                        ["OriginalIssuer"] = c.OriginalIssuer,
                        ["Properties"] = new JObject(c.Properties)
                    }))
                });
            }

            return Json(result);
        }
    }
}

#endif