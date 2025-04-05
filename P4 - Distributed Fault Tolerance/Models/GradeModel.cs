namespace P4___Distributed_Fault_Tolerance.Models
{
    public class GradeModel
    {
        public int GradeId { get; set; }
        public string StudentId { get; set; }
        public string CourseId { get; set; }
        public string GradeValue { get; set; }

        // Add these new fields
        public string FirstName { get; set; }  // First Name of the student
        public string LastName { get; set; }   // Last Name of the student
        public string CourseName { get; set; } // Full name of the course
        public string CourseCode { get; set; }
    }
}
