using System;

namespace Client.Data
{
    public class ScholarshipApplicant
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public double AverageGrade { get; set; }

        public int Ects { get; set; }

        public DateTime Date { get; set; }
    }
}
