using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Client.Models
{
    public class Transcript
    {
        public List<CompletedCourse> Courses { get; set; }

        public int TotalEcts => Courses.Sum(c => c.Ects);

        public double AverageGrade => Courses.Average(c => c.Grade);

        public bool IsEligibleForScholarship => TotalEcts >= 30 && AverageGrade >= 8.0d;
    }

    public class CompletedCourse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("ects")]
        public int Ects { get; set; }

        [JsonProperty("grade")]
        public int Grade { get; set; }
    }
}
