using System;
using System.Threading.Tasks;
using AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Utils.Initialization;

namespace AuthServer.Infrastructure.Initialization
{
    public class UserInitializer : IStartupService
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger logger;

        public UserInitializer(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<UserInitializer> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task InitializeAsync()
        {
            await AddRole(AppConstants.Roles.Administrator);
            await AddRole(AppConstants.Roles.Application);
            await AddDefaultUser();
        }

        private async Task AddRole(string role)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                logger.LogDebug($"Role `{role}` exists.");
                return;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
            {
                logger.LogDebug($"Created the role `{role}` successfully.");
            }
            else
            {
                var exception = new ApplicationException($"Default role `{role}` cannot be created.");
                logger.LogError(exception, Errors(result));
                throw exception;
            }
        }

        private async Task AddDefaultUser()
        {
            var email = AppConstants.AdminEmail;
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return;
            }

            var result = await userManager.CreateAsync(user = new ApplicationUser { Email = email, UserName = email }, email);

            if (result.Succeeded)
            {
                logger.LogDebug($"Created user `{email}` successfully.");
            }
            else
            {
                var exception = new ApplicationException($"Default user `{email}` cannot be created.");
                logger.LogError(exception, Errors(result));
                throw exception;
            }

            result = await userManager.AddToRoleAsync(user, AppConstants.Roles.Administrator);

            if (result.Succeeded)
            {
                logger.LogDebug($"Added adminstrator role to `{email}`.");
            }
            else
            {
                var exception = new ApplicationException($"Could not add `{email}` to administrator role.");
                logger.LogError(exception, Errors(result));
                throw exception;
            }
        }

        private static string Errors(IdentityResult result)
        {
            var errors = "";
            foreach (var identityError in result.Errors)
            {
                errors += identityError.Description;
                errors += ", ";
            }

            return errors;
        }
    }
}
