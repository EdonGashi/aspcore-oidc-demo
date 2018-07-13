using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AuthServer.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Controllers
{
    [ApiVersionNeutral, ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            await LoadTokens();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
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

        private async Task LoadTokens()
        {
            var currentUser = await userManager.GetUserAsync(User);
            var hasTokens = false;
            if (currentUser != null)
            {
                var token = await userManager.GetAuthenticationTokenAsync(currentUser, "Google", "access_token");
                hasTokens = token != null;
                ViewData["access_token"] = token;
                token = await userManager.GetAuthenticationTokenAsync(currentUser, "Google", "id_token");
                hasTokens |= token != null;
                ViewData["id_token"] = token;
                token = await userManager.GetAuthenticationTokenAsync(currentUser, "Google", "refresh_token");
                hasTokens |= token != null;
                ViewData["refresh_token"] = token;
            }

            ViewData["has_tokens"] = hasTokens;
        }
    }
}
