using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using ResourceServer.Models;
using Utils.Helpers;

namespace ResourceServer.Controllers
{
    [ApiVersionNeutral, ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly IConfiguration configuration;

        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult NotRegistered()
        {
            return View();
        }

        public IActionResult Login([FromQuery] string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(returnUrl);
            }

            var loginUrl = PathUtils.Join(configuration["AuthServer:BaseUrl"], configuration["AuthServer:LoginPath"]);
            returnUrl = returnUrl ?? Url.Content("~/");
            return Redirect(QueryHelpers.AddQueryString(loginUrl, new Dictionary<string, string>
            {
                ["returnUrl"] = returnUrl,
                ["server"] = "resource_server"
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Logout([FromQuery] string returnUrl)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return LocalRedirect(returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                Error = "An error has occurred.",
                ErrorDescription = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
