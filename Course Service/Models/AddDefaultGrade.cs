namespace Course_Service.Models
{
    public class AddDefaultGrade
    {
        public string StudentId { get; set; }
        public string CourseId { get; set; }
        public string GradeValue { get; set; } = "0"; // Default grade value is "0"
    }
}
