using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResourceServer.Data;
using ResourceServer.Models;
using Utils;

namespace ResourceServer.Controllers
{
    [ApiVersionNeutral, ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = AppConstants.Roles.Student)]
    [Route("[controller]/[action]")]
    public class StudentCoursesController : Controller
    {
        private readonly ApplicationDbContext db;

        public StudentCoursesController(ApplicationDbContext db)
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

            if (await db.Students.FindAsync(sub) == null)
            {
                return RedirectToAction("NotRegistered", "Home");
            }

            var enrolledCourses = await db
                .Enrollments
                .Include(e => e.Course.Teacher)
                .Include(e => e.Course.Subject)
                .Where(e => e.StudentId == sub)
                .ToListAsync();

            var subjects = enrolledCourses
                .Select(e => e.Course.SubjectId)
                .ToList();

            var availableCourses = await db
                .Courses
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                .Include(c => c.EnrolledStudents)
                .Where(c => c.EnrolledStudents.All(e => e.StudentId != sub) && !subjects.Contains(c.SubjectId))
                .ToListAsync();

            return View(new MyEnrollmentsViewModel
            {
                EnrolledCourses = enrolledCourses,
                AvailableCourses = availableCourses
            });
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(int id)
        {
            var sub = GetId();
            if (sub == null)
            {
                return BadRequest();
            }

            if (await db.Students.FindAsync(sub) == null)
            {
                return RedirectToAction("NotRegistered", "Home");
            }

            var enrolledCourses = await db
                .Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == sub)
                .ToListAsync();

            var subjects = enrolledCourses
                .Select(e => e.Course.SubjectId)
                .ToList();

            if (!await db
                .Courses
                .Where(c => c.Id == id && c.EnrolledStudents.All(e => e.StudentId != sub) && !subjects.Contains(c.SubjectId))
                .AnyAsync())
            {
                return RedirectToAction(nameof(Index));
            }

            db.Enrollments.Add(new Enrollment
            {
                CourseId = id,
                StudentId = sub
            });

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private string GetId()
        {
            return User.FindFirst(OpenIdConnectConstants.Claims.Subject)?.Value;
        }
    }
}
