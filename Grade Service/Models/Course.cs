namespace Grade_Service.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseSection { get; set; }
        public int Units { get; set; }
        public int Capacity { get; set; }
        public List<string> Students { get; set; }  // Assuming Students is a list of student IDs
        public string ProfId { get; set; }
    }
}
