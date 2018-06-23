using System.Threading.Tasks;
using AuthServer.Data;
using Microsoft.EntityFrameworkCore;
using Utils.Initialization;

namespace AuthServer.Infrastructure.Initialization
{
    [StartupOrder(-1)]
    public class DatabaseInitializer : IStartupService
    {
        private readonly ApplicationDbContext context;

        public DatabaseInitializer(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task InitializeAsync()
        {
            await context.Database.MigrateAsync();
        }
    }
}
