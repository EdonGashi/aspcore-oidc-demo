using System.Collections.Generic;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ResourceServer.Controllers
{
    [ApiController, ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ValuesController : ControllerBase
    {
        private static readonly Dictionary<string, string> Store = new Dictionary<string, string>();

        // GET api/values
        [HttpGet]
        [Authorize("values.read")]
        public ActionResult<string> Get()
        {
            var sub = GetSub();
            if (sub == null)
            {
                return BadRequest();
            }

            return Store.TryGetValue(sub, out var result)
                ? result
                : "Success [Empty value].";
        }

        // GET api/values/{sub}
        [HttpGet("{sub}")]
        [Authorize("values.write", Roles = "administrator")]
        public ActionResult<string> GetForUser([FromRoute] string sub)
        {
            if (string.IsNullOrEmpty(sub))
            {
                return BadRequest();
            }

            return Store.TryGetValue(sub, out var result)
                ? result
                : "Success [Empty value].";
        }

        // PUT api/values/{sub}
        [HttpPut("{sub}")]
        [Authorize("values.write", Roles = "administrator")]
        public IActionResult Post([FromRoute] string sub, [FromBody] string value)
        {
            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(value))
            {
                return BadRequest();
            }

            Store[sub] = value;
            return Ok();
        }

        private string GetSub()
        {
            return HttpContext.User?.FindFirst(OpenIdConnectConstants.Claims.Subject).Value;
        }
    }
}
