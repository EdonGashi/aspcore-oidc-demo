using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceServer.Data
{
    public class Student
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }

    public class Teacher
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Course> Courses { get; set; }
    }

    public class Subject
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Ects { get; set; }
    }

    public class Course
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Teacher))]
        [Display(Name = "Teacher")]
        public string TeacherId { get; set; }

        public virtual Teacher Teacher { get; set; }

        [Required]
        [ForeignKey(nameof(Subject))]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }

        public DateTime Date { get; set; }

        public virtual Subject Subject { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }

    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Student))]
        public string StudentId { get; set; }

        public virtual Student Student { get; set; }

        [Required]
        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }

        public virtual Course Course { get; set; }

        public int? Grade { get; set; }
    }
}
