using System.Collections.Generic;
using System.Linq;
using ResourceServer.Data;

namespace ResourceServer.Models
{
    public class MyEnrollmentsViewModel
    {
        public List<Enrollment> EnrolledCourses { get; set; }

        public List<Course> AvailableCourses { get; set; }

        public IEnumerable<Enrollment> CompletedCourses => EnrolledCourses?.Where(e => e.Grade != null);

        public IEnumerable<Enrollment> CurrentCourses => EnrolledCourses?.Where(e => e.Grade == null);

        public int CurrentEcts => CompletedCourses?.Sum(e => e.Course.Subject.Ects) ?? 0;

        public string CurrentGrade
        {
            get
            {
                var courses = EnrolledCourses?.Where(e => e.Grade != null).ToList();
                return courses == null || courses.Count == 0
                    ? "N/A"
                    : courses.Average(e => e.Grade.Value).ToString("0.00");
            }
        }
    }
}
