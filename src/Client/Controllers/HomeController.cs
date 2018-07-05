using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient client;

        public HomeController(HttpClient client)
        {
            this.client = client;
        }

        [HttpGet("~/")]
        public async Task<ActionResult> Index()
        {
            await LoadTokens();
            return View();
        }

        [Authorize, HttpPost("~/")]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var token = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("The access token cannot be found in the authentication ticket. " +
                                                    "Make sure that SaveTokens is set to true in the OIDC options.");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:4001/api/values");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await LoadTokens();
            return View((object)await response.Content.ReadAsStringAsync());
        }

        [HttpGet("~/error"), ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        private async Task LoadTokens()
        {
            ViewData["access_token"] = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
            ViewData["id_token"] = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "id_token");
            ViewData["refresh_token"] = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "refresh_token");
        }
    }
}
