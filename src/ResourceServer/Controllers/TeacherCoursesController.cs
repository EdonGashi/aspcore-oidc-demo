﻿using System.Linq;
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
    public class TeacherCoursesController : Controller
    {
        private readonly ApplicationDbContext db;

        public TeacherCoursesController(ApplicationDbContext db)
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

            var classes = await db
                .Courses
                .Include(c => c.Subject)
                .Include(c => c.EnrolledStudents)
                .Where(c => c.TeacherId == sub)
                .ToListAsync();

            return View(classes);
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

            var @class = await db
                .Courses
                .Include(c => c.Subject)
                .Include(c => c.EnrolledStudents)
                .ThenInclude(e => e.Student)
                .Where(c => c.TeacherId == sub && c.Id == id)
                .SingleOrDefaultAsync();

            if (@class == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(@class);
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
