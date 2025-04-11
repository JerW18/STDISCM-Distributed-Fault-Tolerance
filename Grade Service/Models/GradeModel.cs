using System.ComponentModel.DataAnnotations;

namespace Grade_Service.Models
{
    public class GradeModel
    {
        [Key]
        public int GradeId { get; set; }      // Unique identifier for the grade entry
        public string Lastname { get; set; } // The last name of the student
        public string Firstname { get; set; } // The first name of the student
        public string StudentId { get; set; } // The ID of the student
        public string CourseId { get; set; }  // The ID of the course
        public string CourseCode { get; set; } // The code of the course
        public string GradeValue { get; set; } // The grade value (e.g., "A", "B+", "C", etc.)
        public int Units { get; set; } // The number of units for the course
        public string ProfId { get; set; } // The ID of the professor
    }
}