using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResourceServer.Data;
using ResourceServer.Models;
using ResourceServer.Services;
using Utils;

namespace ResourceServer.Controllers
{
    [ApiVersionNeutral, ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = AppConstants.Roles.Administrator)]
    [Route("[controller]/[action]")]
    public class ManageUsersController : Controller
    {
        private readonly IUsersService usersService;
        private readonly ApplicationDbContext db;

        public ManageUsersController(IUsersService usersService, ApplicationDbContext db)
        {
            this.usersService = usersService;
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            var users = (await usersService.GetUsersAsync()).ToList();
            var teachers = await db.Teachers.ToListAsync();
            var students = await db.Students.ToListAsync();
            users = users
                .Where(u => teachers.All(t => t.Id != u.Subject) && students.All(s => s.Id != u.Subject))
                .ToList();

            return View(new ImportViewModel
            {
                NewUsers = users,
                Students = students,
                Teachers = teachers
            });
        }

        [HttpPost]
        public async Task<IActionResult> ImportStudent(string id, string name)
        {
            var student = await db.Students.FindAsync(id);
            if (student == null)
            {
                db.Students.Add(new Student
                {
                    Id = id,
                    Name = name
                });

                await db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTeacher(string id)
        {
            db.Teachers.Remove(new Teacher
            {
                Id = id
            });

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            db.Students.Remove(new Student
            {
                Id = id
            });

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ImportTeacher(string id, string name)
        {
            var teacher = await db.Teachers.FindAsync(id);
            if (teacher == null)
            {
                db.Teachers.Add(new Teacher
                {
                    Id = id,
                    Name = name
                });

                await db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ImportAll()
        {
            var users = (await usersService.GetUsersAsync()).ToList();
            var teachers = await db.Teachers.ToListAsync();
            var students = await db.Students.ToListAsync();
            foreach (var user in users)
            {
                if (user.Roles.Count != 1)
                {
                    continue;
                }

                var role = user.Roles[0];
                switch (role)
                {
                    case AppConstants.Roles.Student:
                        if (students.Any(s => s.Id == user.Subject))
                        {
                            continue;
                        }

                        db.Students.Add(new Student
                        {
                            Id = user.Subject,
                            Name = user.Email
                        });

                        break;

                    case AppConstants.Roles.Teacher:
                        if (teachers.Any(t => t.Id == user.Subject))
                        {
                            continue;
                        }

                        db.Teachers.Add(new Teacher
                        {
                            Id = user.Subject,
                            Name = user.Email
                        });

                        break;

                    default:
                        continue;
                }
            }

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
