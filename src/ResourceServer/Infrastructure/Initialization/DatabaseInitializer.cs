using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ResourceServer.Data;
using Utils.Initialization;

namespace ResourceServer.Infrastructure.Initialization
{
    [StartupOrder(-1)]
    public class DatabaseInitializer : IStartupService
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger logger;

        public DatabaseInitializer(ApplicationDbContext context, ILogger<DatabaseInitializer> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                logger.LogDebug("Attempting to migrate database.");
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating database.");
            }
        }
    }
}
