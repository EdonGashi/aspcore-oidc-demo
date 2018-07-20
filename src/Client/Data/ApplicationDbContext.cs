using Microsoft.EntityFrameworkCore;

namespace Client.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<ScholarshipApplicant> ScholarshipApplicants { get; set; }
    }
}
