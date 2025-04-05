using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Course_Service.Models
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int CourseId { get; set; }
        public required string CourseCode { get; set; }
        public required string CourseName { get; set; }
        public required string CourseSection { get; set; }
        public required int Units { get; set; }
        public required int Capacity { get; set; }
        public required List<string> Students { get; set; }
        public required string ProfId { get; set; } 

    }
}
