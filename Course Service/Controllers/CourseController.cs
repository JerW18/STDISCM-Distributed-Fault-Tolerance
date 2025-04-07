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
                FullCourseName.CourseCode,
                FullCourseName.Units
            });

        }

        [HttpGet]
        [Route("getCourses")]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _context.Courses.ToListAsync();
            if (courses == null || !courses.Any())
            {
                return NotFound(new { message = "No available courses." });
            }
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
        [Route("getAvailCourses")]
        public async Task<IActionResult> GetCourses(EnrollRequest enrollrequest)
        {
            var allCourses = await _context.Courses.ToListAsync();
            var filteredCourses = allCourses
                .Where(c => !c.Students.Contains(enrollrequest.IdNumber) &&
                            c.Students.Count < c.Capacity)
                .ToList();
            if (filteredCourses == null || !filteredCourses.Any()){
                return NotFound(new { message = "No available courses to enroll in." });
            }
            return Ok(filteredCourses);
        }

        [HttpPost]
        [Route("addCourse")]
        [AllowAnonymous]
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
            var courseIds = _context.Courses
                .Where(c => c.ProfId == profId)
                .Select(c => c.CourseId)
                .ToList();
            return Ok(courseIds); 
        }

        [HttpGet("findcourse/{courseId}")]
        public IActionResult GetCourseDetails(int courseId)
        {
            var course = _context.Courses
                .Where(c => c.CourseId == courseId)
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseCode,
                    c.CourseName,
                    c.CourseSection,
                    c.Units,
                    c.Capacity,
                    Students = c.Students, 
                    c.ProfId
                })
                .FirstOrDefault();
            return Ok(course); 
        }
    }
}
