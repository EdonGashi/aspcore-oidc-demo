using DynamicData.Implementations;
using DynamicData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DynamicData.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDynamicDataStores<TContext, TUser>(this IServiceCollection services)
            where TContext : DbContext
            where TUser : class
        {
            services.TryAddScoped<IApplicationPropertyManager, ApplicationPropertyManager<TContext, ApplicationProperty>>();
            services.TryAddScoped<IUserApplicationPropertyManagerFactory, Factory<TContext, UserApplicationProperty<TUser>, TUser>>();
            services.TryAddScoped<IUserPersonalPropertyManagerFactory, Factory<TContext, UserPersonalProperty<TUser>, TUser>>();
            services.TryAddScoped<IUserClientPropertyManagerFactory, Factory<TContext, UserClientProperty<TUser>, TUser>>();
        }
    }
}
