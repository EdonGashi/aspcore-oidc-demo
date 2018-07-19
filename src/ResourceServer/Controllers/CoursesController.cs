using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResourceServer.Data;
using Utils;

namespace ResourceServer.Controllers
{
    [ApiVersionNeutral, ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = AppConstants.Roles.Administrator)]
    [Route("[controller]/[action]")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext db;

        public CoursesController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(await db.Courses
                .Include(course => course.Subject)
                .Include(course => course.Teacher)
                .ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadRelatedEntities();
            return View(new Course());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Course course)
        {
            if (!ModelState.IsValid)
            {
                await LoadRelatedEntities();
                return View(course);
            }

            course.Date = DateTime.Now;
            db.Courses.Add(course);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            db.Courses.Remove(new Course
            {
                Id = id
            });

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadRelatedEntities()
        {
            ViewData["subjects"] = await db.Subjects.ToListAsync();
            ViewData["teachers"] = await db.Teachers.ToListAsync();
        }
    }
}
