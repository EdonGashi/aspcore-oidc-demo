using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AuthServer.Controllers
{
    [ApiVersionNeutral, ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = AppConstants.Roles.Administrator)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var dbUsers = userManager
                .Users
                .ToList();

            var users = new List<UserViewModel>(dbUsers.Count);
            foreach (var user in dbUsers)
            {
                var roles = await userManager.GetRolesAsync(user);
                var isAdmin = roles.Contains(AppConstants.Roles.Administrator);
                users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = string.Join(", ", roles),
                    IsAdmin = isAdmin
                });
            }

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromForm] string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (await userManager.IsInRoleAsync(user, AppConstants.Roles.Administrator))
                {
                    return BadRequest();
                }

                await userManager.DeleteAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            result = await userManager.AddToRoleAsync(user, model.Role);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unknown error.");
            return View(model);
        }
    }
}
