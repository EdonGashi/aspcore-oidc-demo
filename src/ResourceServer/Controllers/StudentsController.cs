using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceServer.Data;
using ResourceServer.Services;
using Utils;

namespace ResourceServer.Controllers
{
    [ApiVersionNeutral, ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = AppConstants.Roles.Administrator + "," + AppConstants.Roles.Teacher)]
    [Route("[controller]/[action]")]
    public class StudentsController : Controller
    {
        private readonly IStudentsService studentsService;
        private readonly ApplicationDbContext context;

        public StudentsController(IStudentsService studentsService, ApplicationDbContext context)
        {
            this.studentsService = studentsService;
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            var students = await studentsService.GetStudentsAsync();
            return View(students);
        }
    }
}
