#if DEBUG

using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AuthServer.V1.Controllers
{
    [ApiController, ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DebugController : Controller
    {
        [Authorize]
        [HttpGet("me"), Produces("application/json")]
        public IActionResult GetCurrentUserInfo()
        {
            var identities = new JArray();
            var result = new JObject
            {
                ["identities"] = identities,
                ["claims"] = new JArray(User.Claims.Select(c => new JArray(c.Type, c.Value)))
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