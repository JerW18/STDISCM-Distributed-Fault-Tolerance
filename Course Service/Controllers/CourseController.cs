using System.Diagnostics;
using Course_Service.Data;
using Course_Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Course_Service.Controllers
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}
