namespace Grade_Service.Models
{
    public class DefaultGrade
    {
        public string StudentId { get; set; }
        public string CourseId { get; set; }
        public string GradeValue { get; set; } = "0"; // Default grade value is "0"
    }
}
