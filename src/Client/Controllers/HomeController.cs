using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Client.Data;
using Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Utils.Helpers;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger logger;
        private readonly ApplicationDbContext db;
        private readonly IConfiguration configuration;
        private readonly HttpClient client;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IConfiguration configuration, HttpClient client)
        {
            this.logger = logger;
            this.db = db;
            this.configuration = configuration;
            this.client = client;
        }

        [HttpGet("~/")]
        public async Task<ActionResult> Index()
        {
            var scholarshipWinners = await db.ScholarshipApplicants
                .OrderBy(w => w.AverageGrade)
                .ThenBy(w => w.Ects)
                .ToListAsync();
            return View(scholarshipWinners);
        }

        [HttpPost("~/initiateapply")]
        public async Task<IActionResult> InitiateApply()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Apply));
        }

        [Authorize(AuthenticationSchemes = "AuthServer")]
        [HttpGet("~/apply")]
        public async Task<IActionResult> Apply()
        {
            var transcript = await GetTranscript(HttpContext.RequestAborted);
            if (transcript == null)
            {
                return RedirectToAction(nameof(Forbidden));
            }

            var subject = GetId();
            if (subject == null)
            {
                return RedirectToAction(nameof(Forbidden));
            }

            var existing = db.ScholarshipApplicants.Find(subject);
            if (existing != null)
            {
                return RedirectToAction(nameof(Applied));
            }

            return View(transcript);
        }

        [Authorize, HttpPost("~/apply")]
        public async Task<ActionResult> ConfirmApply()
        {
            var transcript = await GetTranscript(HttpContext.RequestAborted);
            if (transcript == null)
            {
                return RedirectToAction(nameof(Forbidden));
            }

            var subject = GetId();
            if (subject == null)
            {
                return RedirectToAction(nameof(Forbidden));
            }

            if (!transcript.IsEligibleForScholarship)
            {
                return RedirectToAction(nameof(Forbidden));
            }

            var existing = db.ScholarshipApplicants.Find(subject);
            if (existing != null)
            {
                return RedirectToAction(nameof(Applied));
            }

            db.ScholarshipApplicants.Add(new ScholarshipApplicant
            {
                Id = subject,
                Name = User.FindFirstValue(OpenIdConnectConstants.Claims.Email) ?? User.Identity.Name,
                AverageGrade = transcript.AverageGrade,
                Ects = transcript.TotalEcts,
                Date = DateTime.Now
            });

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Success));
        }

        [HttpGet("~/success")]
        public async Task<ActionResult> Success()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpGet("~/forbidden")]
        public async Task<ActionResult> Forbidden()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpGet("~/applied")]
        public async Task<ActionResult> Applied()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpGet("~/error"), ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        private async Task<Transcript> GetTranscript(CancellationToken cancellationToken)
        {
            try
            {
                var token = await HttpContext.GetTokenAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    "access_token");

                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException(
                        "The access token cannot be found in the authentication ticket. " +
                        "Make sure that SaveTokens is set to true in the OIDC options.");
                }

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    PathUtils.Join(configuration["ResourceServer:BaseUrl"], "api/v1/transcript"));

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return new Transcript
                {
                    Courses = JsonConvert.DeserializeObject<List<CompletedCourse>>(
                        await response.Content.ReadAsStringAsync())
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending backchannel request.");
                return null;
            }
        }

        private string GetId()
        {
            return User.FindFirst(OpenIdConnectConstants.Claims.Subject)?.Value;
        }
    }
}
