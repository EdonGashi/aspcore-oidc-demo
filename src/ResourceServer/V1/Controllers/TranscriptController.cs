using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ResourceServer.Data;
using Utils;

namespace ResourceServer.V1.Controllers
{
    [ApiController, ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TranscriptController : Controller
    {
        private readonly ApplicationDbContext db;

        public TranscriptController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpGet, Authorize(AppConstants.Scopes.TranscriptRead, Roles = AppConstants.Roles.Student)]
        public async Task<IActionResult> MyTranscript()
        {
            var sub = GetId();
            if (sub == null)
            {
                return Forbid();
            }

            if (await db.Students.FindAsync(sub) == null)
            {
                return NotFound();
            }

            var completedCourses = await db
                .Enrollments
                .Include(e => e.Course.Subject)
                .Where(e => e.StudentId == sub && e.Grade != null)
                .ToListAsync();

            return Json(new JArray(
                completedCourses
                    .Select(c => new JObject
                    {
                        ["id"] = c.CourseId,
                        ["name"] = c.Course.Subject.Name,
                        ["year"] = c.Course.Date.Year,
                        ["ects"] = c.Course.Subject.Ects,
                        ["grade"] = c.Grade
                    })));
        }

        private string GetId()
        {
            return User.FindFirst(OpenIdConnectConstants.Claims.Subject)?.Value;
        }
    }
}
