using System.Diagnostics;
using Course_Service.Data;
using Course_Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Course_Service.Controllers
{
    [Route("api/course")]
    [ApiController]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("coursename/{courseName}")]
        public IActionResult GetCourseName(string courseName)
        {
            // Logging to confirm the method is reached
            Trace.WriteLine($"Searching for course with name: {courseName}");

            // Handle case-insensitive search (optional, depending on your needs)
            var FullCourseName = _context.Courses
                .FirstOrDefault(s => s.CourseId.ToString().ToLower() == courseName.Trim().ToLower());

            var courseFromDb = _context.Courses.FirstOrDefault();
            Trace.WriteLine($"Stored course name: '{courseFromDb?.CourseName}'");

            if (FullCourseName == null)
            {
                Trace.WriteLine($"Course with name {courseName} not found");
                return NotFound(new { message = "Course not found" });
            }

            return Ok(new
            {
                FullCourseName.CourseName,
                FullCourseName.CourseCode

            });

        }

        [HttpGet]
        [Route("getCourses")]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _context.Courses.ToListAsync();
            return Ok(courses);
        }

        [HttpPost]
        [Route("enrollStudent")]
        public async Task<IActionResult> EnrollStudent([FromBody] EnrollRequest enrollRequest)
        {
            var existingCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == enrollRequest.CourseId);

            if (existingCourse == null)
            {
                return NotFound(new { message = "Course not found!" });
            }

            if (existingCourse.Students.Count >= existingCourse.Capacity)
            {
                return BadRequest(new { message = "Course is full!" });
            }

            if (existingCourse.Students.Contains(enrollRequest.IdNumber))
            {
                return BadRequest(new { message = "Student already enrolled!" });
            }

            existingCourse.Students.Add(enrollRequest.IdNumber);
            _context.Courses.Update(existingCourse);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Student enrolled successfully!" });
        }

        [HttpPost]
        [Route("addCourse")]
        public async Task<IActionResult> AddCourse([FromBody] Course course)
        {
            if (course == null)
            {
                return BadRequest(new { message = "Invalid course data!" });
            }
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCourses), new { id = course.CourseId }, course);
        }

        [HttpGet("profclass/{profId}")]
        public IActionResult GetProfClasses(string profId)
        {
            Trace.WriteLine($"Searching for course with profId: {profId}");

            var allClasses = _context.Courses
                .Where(s => s.ProfId == profId)
                .Select(c => c.CourseId.ToString()) // Ensure CourseId is a string
                .ToList();

            if (!allClasses.Any())
            {
                Trace.WriteLine($"No courses found for professor {profId}");
                return NotFound(new { message = "No courses found." });
            }

            return Ok(allClasses);
        }
    }
}
