using DynamicData.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicData.Extensions
{
    public static class EntityFrameworkCoreExtensions
    {
        public static ModelBuilder UseDynamicData<TUser>(this ModelBuilder builder) where TUser : class
        {
            builder.Entity<UserPersonalProperty<TUser>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.Key });
                entity.ToTable("DynamicDataUserPersonalProperties");
            });

            builder.Entity<UserApplicationProperty<TUser>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.Key });
                entity.ToTable("DynamicDataUserApplicationProperties");
            });

            builder.Entity<UserClientProperty<TUser>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.Key });
                entity.ToTable("DynamicDataUserClientProperties");
            });

            builder.Entity<ApplicationProperty>(entity =>
            {
                entity.ToTable("DynamicDataApplicationProperties");
            });

            return builder;
        }
    }
}
