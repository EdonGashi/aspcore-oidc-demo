using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ResourceServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Grade>(entity
                => entity.HasKey(grade => new { grade.StudentId, grade.SubjectId }));
        }

        public DbSet<Subject> Subjects { get; set; }

        public DbSet<Grade> Grades { get; set; }
    }

    public class Subject
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Ects { get; set; }
    }

    public class Grade
    {
        public string StudentId { get; set; }

        [ForeignKey(nameof(Subject))]
        public int SubjectId { get; set; }

        public Subject Subject { get; set; }

        public int GradeValue { get; set; }
    }
}
