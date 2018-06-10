using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Utils.Initialization
{
    public static class AssemblyExtensions
    {
        public static async Task InitializeAssembly(this IServiceProvider serviceProvider, Assembly assembly)
        {
            var iStartupService = typeof(IStartupService);
            var types = assembly
                .GetTypes()
                .Where(p => iStartupService.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

            foreach (var type in types)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var startupService = (IStartupService)ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);
                    await startupService.InitializeAsync();
                    if (startupService is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}
