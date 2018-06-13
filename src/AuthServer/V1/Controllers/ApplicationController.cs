using System;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AuthServer.V1.Controllers
{
    [ApiController, ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ApplicationController : Controller
    {
        private readonly IApplicationPropertyManager propertyManager;

        public ApplicationController(IApplicationPropertyManager propertyManager)
        {
            this.propertyManager = propertyManager;
        }

        [Authorize(AppConstants.Scopes.ApplicationRead, Roles = AppConstants.Roles.Administrator)]
        [HttpGet("data/{key}"), Produces("application/json")]
        public async Task<IActionResult> GetValue([FromRoute] string key)
        {
            return Json(await propertyManager.Get(key));
        }

        [Authorize(AppConstants.Scopes.ApplicationRead, Roles = AppConstants.Roles.Administrator)]
        [HttpGet("data"), Produces("application/json")]
        public async Task<IActionResult> GetValues([FromQuery] string keys)
        {
            string[] keysarr = null;
            if (!string.IsNullOrWhiteSpace(keys))
            {
                keysarr = keys
                    .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(val => val.Trim())
                    .ToArray();
            }

            if (keysarr == null || keysarr.Length == 0)
            {
                return Json(await propertyManager.GetAll());
            }

            return Json(await propertyManager.Get(keysarr));
        }

        [Authorize(AppConstants.Scopes.ApplicationWrite, Roles = AppConstants.Roles.Administrator)]
        [HttpPut("data/{key}")]
        public async Task<IActionResult> SetValue([FromRoute] string key, [FromBody] JToken value)
        {
            if (value == null)
            {
                return BadRequest();
            }

            await propertyManager.Set(key, value);
            return Ok();
        }

        [Authorize(AppConstants.Scopes.ApplicationWrite, Roles = AppConstants.Roles.Administrator)]
        [HttpDelete("data/{key}")]
        public async Task<IActionResult> DeleteValue([FromRoute] string key)
        {
            await propertyManager.Remove(key);
            return Ok();
        }

        [Authorize(AppConstants.Scopes.ApplicationWrite, Roles = AppConstants.Roles.Administrator)]
        [HttpDelete("data")]
        public async Task<IActionResult> DeleteValues([FromQuery] string keys)
        {
            string[] keysarr = null;
            if (!string.IsNullOrWhiteSpace(keys))
            {
                keysarr = keys
                    .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(val => val.Trim())
                    .ToArray();
            }

            if (keysarr == null || keysarr.Length == 0)
            {
                return BadRequest();
            }

            await propertyManager.Remove(keysarr);
            return Ok();
        }

        [Authorize(AppConstants.Scopes.ApplicationWrite, Roles = AppConstants.Roles.Administrator)]
        [HttpPatch("data")]
        public async Task<IActionResult> SetValues([FromBody] JToken value)
        {
            if (!(value is JObject obj))
            {
                return BadRequest();
            }

            var prev = propertyManager.AutoSave;
            propertyManager.AutoSave = false;
            foreach (var kvp in obj)
            {
                await propertyManager.Set(kvp.Key, kvp.Value);
            }

            await propertyManager.SaveAsync();
            propertyManager.AutoSave = prev;
            return Ok();
        }
    }
}
