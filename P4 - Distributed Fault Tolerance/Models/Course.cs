namespace P4___Distributed_Fault_Tolerance.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public required string CourseCode { get; set; }
        public required string CourseName { get; set; }
        public required string CourseSection { get; set; }
        public required int Units { get; set; }
        public required int Capacity { get; set; }
        public required List<string> Students { get; set; }
    }
}
