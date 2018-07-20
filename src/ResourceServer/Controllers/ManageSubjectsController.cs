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
    public class ManageSubjectsController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public ManageSubjectsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            return View(await dbContext.Subjects.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Subject subject)
        {
            if (!ModelState.IsValid)
            {
                return View(subject);
            }

            dbContext.Subjects.Add(subject);
            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromForm] int id)
        {
            dbContext.Subjects.Remove(new Subject
            {
                Id = id
            });

            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
