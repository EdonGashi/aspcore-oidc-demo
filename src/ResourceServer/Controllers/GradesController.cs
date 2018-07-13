using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResourceServer.Data;
using ResourceServer.Models;
using Utils;

namespace ResourceServer.Controllers
{
    [ApiVersionNeutral, ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = AppConstants.Roles.Teacher)]
    [Route("[controller]/[action]")]
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public GradesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index([FromRoute] string id)
        {
            var grades = dbContext.Grades
                .Include(grade => grade.Subject)
                .Where(grade => grade.StudentId == id);

            ViewData["id"] = id;
            return View(await grades.ToListAsync());
        }

        [HttpGet("{id}")]
        public IActionResult Add([FromRoute] string id)
        {
            var grades = dbContext.Grades
                .Where(grade => grade.StudentId == id)
                .ToList();

            ViewData["subjects"] = dbContext.Subjects.Where(subject => grades.All(g => g.SubjectId != subject.Id));
            return View(new AddGradeViewModel
            {
                StudentId = id
            });
        }

        [HttpPost("{id?}")]
        public async Task<IActionResult> Add(AddGradeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var grades = dbContext.Grades
                    .Where(grade => grade.StudentId == model.StudentId)
                    .ToList();
                ViewData["subjects"] = dbContext.Subjects.Where(subject => grades.All(g => g.SubjectId != subject.Id));
                return View(model);
            }

            dbContext.Grades.Add(new Grade
            {
                StudentId = model.StudentId,
                SubjectId = model.SubjectId ?? throw new InvalidOperationException(),
                GradeValue = model.Grade ?? throw new InvalidOperationException()
            });

            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = model.StudentId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromForm] string studentid, [FromForm] string subjectid)
        {
            var grade = await dbContext.Grades.FindAsync(studentid, subjectid);
            if (grade == null)
            {
                return RedirectToAction(nameof(Index), new { id = studentid });
            }

            dbContext.Grades.Remove(grade);
            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = studentid });
        }
    }
}
