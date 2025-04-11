namespace Grade_Service.Models
{
    public class GradeDto
    {
        public string StudentId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string CourseId { get; set; }
        public string CourseCode { get; set; }
        public string Grade { get; set; }
        public int Units { get; set; }
        public string ProfId { get; set; } 
    }
}
