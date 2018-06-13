using AuthServer.Data.DynamicProperties;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //builder.Entity<UserProperty>().HasKey(prop => new { prop.UserId, prop.Key });
        }

        //public DbSet<ApplicationProperty> ApplicationProperties { get; set; }

        //public DbSet<PersonalUserProperty> UserPersonalProperties { get; set; }

        //public DbSet<UserApplicationProperty> UserApplicationProperties { get; set; }

        //public DbSet<UserClientProperty> UserClientProperties { get; set; }
    }
}
