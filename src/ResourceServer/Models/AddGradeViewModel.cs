using System.ComponentModel.DataAnnotations;

namespace ResourceServer.Models
{
    public class AddGradeViewModel
    {
        public string StudentId { get; set; }

        [Required, Display(Name = "Subject")]
        public int? SubjectId { get; set; }

        [Required, Range(5, 10)]
        public int? Grade { get; set; }
    }
}
