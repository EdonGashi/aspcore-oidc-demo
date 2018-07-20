using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResourceServer.Data;
using Utils;

namespace ResourceServer.Controllers
{
    [ApiVersionNeutral, ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = AppConstants.Roles.Teacher)]
    [Route("[controller]/[action]")]
    public class MyCoursesController : Controller
    {
        private readonly ApplicationDbContext db;

        public MyCoursesController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            var sub = GetId();
            if (sub == null)
            {
                return BadRequest();
            }

            if (await db.Teachers.FindAsync(sub) == null)
            {
                return RedirectToAction("NotRegistered", "Home");
            }

            var courses = await db
                .Courses
                .Include(c => c.Subject)
                .Include(c => c.Enrollments)
                .Where(c => c.TeacherId == sub)
                .ToListAsync();

            return View(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var sub = GetId();
            if (sub == null)
            {
                return BadRequest();
            }

            if (await db.Teachers.FindAsync(sub) == null)
            {
                return RedirectToAction("NotRegistered", "Home");
            }

            var course = await db
                .Courses
                .Include(c => c.Subject)
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .Where(c => c.TeacherId == sub && c.Id == id)
                .SingleOrDefaultAsync();

            if (course == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> SetGrade(int id, int grade)
        {
            var enrollment = await GetEnrollment(id);
            if (enrollment == null)
            {
                return RedirectToAction(nameof(Index));
            }

            enrollment.Grade = grade;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = enrollment.CourseId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveGrade(int id)
        {
            var enrollment = await GetEnrollment(id);
            if (enrollment == null)
            {
                return RedirectToAction(nameof(Index));
            }

            enrollment.Grade = null;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = enrollment.CourseId });
        }

        private async Task<Enrollment> GetEnrollment(int id)
        {
            var sub = GetId();
            if (sub == null)
            {
                return null;
            }

            var enrollment = await db
                .Enrollments
                .Where(e => e.Id == id && e.Course.TeacherId == sub)
                .SingleOrDefaultAsync();

            return enrollment;
        }

        private string GetId()
        {
            return User.FindFirst(OpenIdConnectConstants.Claims.Subject)?.Value;
        }
    }
}
