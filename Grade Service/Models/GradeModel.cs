using System.ComponentModel.DataAnnotations;

namespace Grade_Service.Models
{
    public class GradeModel
    {
        [Key]
        public int GradeId { get; set; }      // Unique identifier for the grade entry
        public string StudentId { get; set; } // The ID of the student
        public string CourseName { get; set; }  // The ID of the course
        public string GradeValue { get; set; } // The grade value (e.g., "A", "B+", "C", etc.)
    }
}