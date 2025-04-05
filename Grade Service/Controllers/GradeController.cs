using Grade_Service.Data;
using Grade_Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http;

namespace Grade_Service.Controllers
{
    [Route("api/grades")]
    [ApiController]
    public class GradeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public GradeController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }



        //[HttpPost("getGrades")]
        //public async Task<IActionResult> GetGrades([FromBody] ViewGradeRequest viewGradeRequest)
        //{
        //    if (string.IsNullOrEmpty(viewGradeRequest.IdNumber))
        //    {
        //        return BadRequest(new { message = "Student ID is required." });
        //    }

        //    var grades = await _context.Grades
        //        .Where(g => g.StudentId == viewGradeRequest.IdNumber)
        //        .ToListAsync();

        //    if (!grades.Any())
        //    {
        //        return NotFound(new { message = "No grades found for this student." });
        //    }

        //    return Ok(grades);
        //}

        [HttpPost("getGrades")]
        public async Task<IActionResult> GetGrades([FromBody] ViewGradeRequest viewGradeRequest)
        {
            if (string.IsNullOrEmpty(viewGradeRequest.IdNumber))
            {
                return BadRequest(new { message = "Student ID is required." });
            }

            // First check if grades exist
            var grades = await _context.Grades
                .Where(g => g.StudentId == viewGradeRequest.IdNumber)
                .ToListAsync();

            if (!grades.Any())
            {
                return NotFound(new { message = "No grades found for this student." });
            }

            using var courseClient = new HttpClient();
            courseClient.BaseAddress = new Uri("https://localhost:5002");

            var results = new List<object>();

            foreach (var grade in grades)
            {
                try
                {
                    // Get course details for EACH grade
                    var course = await courseClient.GetFromJsonAsync<CourseDto>(
                        $"/api/course/coursename/{grade.CourseId}"); // Changed endpoint to use course ID

                    results.Add(new
                    {
                        grade.GradeId,
                        grade.CourseId,
                        grade.GradeValue,
                        CourseName = course?.CourseName,
                        CourseCode = course?.CourseCode
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        grade.GradeId,
                        grade.CourseId,
                        grade.GradeValue,
                        CourseName = "N/A",
                        CourseCode = "N/A"
                    });
                }
            }

            return Ok(results);
        }

        [HttpPost("getAllGrades")]
        public async Task<IActionResult> GetAllGrades([FromBody] ViewGradeRequest viewGradeRequest)
        {
            if (string.IsNullOrEmpty(viewGradeRequest.IdNumber))
            {
                return BadRequest(new { message = "Prof ID is required." });
            }

            using var profClient = new HttpClient();
            profClient.BaseAddress = new Uri("https://localhost:5002"); // Course Service URL

            // Get the list of course IDs the professor teaches from the API
            var courseIdsTheProfTeaches = await profClient.GetFromJsonAsync<List<string>>($"/api/course/profclass/{viewGradeRequest.IdNumber}");

            if (courseIdsTheProfTeaches == null || !courseIdsTheProfTeaches.Any())
            {
                return NotFound(new { message = "No courses found for the specified professor." });
            }

            // Get the grades for the courses the professor is teaching
            var grades = await _context.Grades
                .Where(g => courseIdsTheProfTeaches.Contains(g.CourseId))
                .ToListAsync();


            if (!grades.Any())
            {
                return NotFound(new { message = "No grades found." });
            }

            var results = new List<object>();

            foreach (var grade in grades)
            {
                try
                {
                    // Create a new HttpClient instance for Auth Service
                    using var authClient = new HttpClient();
                    authClient.BaseAddress = new Uri("https://localhost:5001"); // Auth Service URL
                    var student = await authClient.GetFromJsonAsync<StudentDto>($"/api/auth/students/{grade.StudentId}");

                    // Create another HttpClient instance for Course Service
                    using var courseClient = new HttpClient();
                    courseClient.BaseAddress = new Uri("https://localhost:5002"); // Course Service URL
                    var course = await courseClient.GetFromJsonAsync<CourseDto>($"/api/course/coursename/{grade.CourseId}");

                    results.Add(new
                    {
                        grade.GradeId,
                        grade.StudentId,
                        grade.CourseId,
                        CourseCode = course?.CourseCode,
                        CourseName = course?.CourseName,
                        grade.GradeValue,
                        FirstName = student?.FirstName,
                        LastName = student?.LastName
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        grade.GradeId,
                        grade.StudentId,
                        grade.CourseId,
                        CourseCode = "N/A",
                        CourseName = "N/A",
                        grade.GradeValue,
                        FirstName = "N/A",
                        LastName = "N/A"
                    });
                }
            }

            return Ok(results);
        }


        public class StudentDto
        {
            public string IdNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class CourseDto
        {
            public string CourseId { get; set; }
            public string CourseCode { get; set; }
            public string CourseName { get; set; } 
        }
    }
}
