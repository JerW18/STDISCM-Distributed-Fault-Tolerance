using Grade_Service.Data;
using Grade_Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        [HttpPost("getAllGradesProf")]
        public async Task<IActionResult> GetAllGradesAsyncProf([FromBody] ViewGradeRequest viewGradeRequest)
        {
            if (string.IsNullOrEmpty(viewGradeRequest.IdNumber))
            {
                return BadRequest(new { message = "Professor ID is required." });
            }

            using var profClient = _httpClientFactory.CreateClient();
            profClient.BaseAddress = new Uri("https://localhost:5002"); // Course Service URL

            var courseIdsTheProfTeaches = await profClient.GetFromJsonAsync<List<int>>($"/api/course/profclass/{viewGradeRequest.IdNumber}");

            if (courseIdsTheProfTeaches == null || !courseIdsTheProfTeaches.Any())
            {
                return NotFound(new { message = "No courses found for the specified professor." });
            }

            var courseIdStrings = courseIdsTheProfTeaches.Select(id => id.ToString()).ToList();

            // Fetch existing grades for the professor's courses
            var existingGradePairs = await _context.Grades
                .Where(g => courseIdStrings.Contains(g.CourseId))
                .Select(g => new { g.StudentId, g.CourseId })
                .ToListAsync();

            var existingGradeSet = new HashSet<Tuple<string, string>>(); // Note: Both are strings now
            foreach (var pair in existingGradePairs)
            {
                existingGradeSet.Add(Tuple.Create(pair.StudentId, pair.CourseId));
            }

            var studentCourseList = new List<object>();

            foreach (var courseId in courseIdsTheProfTeaches)
            {
                try
                {
                    var course = await profClient.GetFromJsonAsync<Course>($"/api/course/findcourse/{courseId}");

                    if (course != null)
                    {
                        foreach (var studentId in course.Students)
                        {
                            var gradeKey = Tuple.Create(studentId, courseId.ToString());
                            if (existingGradeSet.Contains(gradeKey))
                            {
                                continue; // Skip if grade exists
                            }

                            using var authClient = _httpClientFactory.CreateClient();
                            authClient.BaseAddress = new Uri("https://localhost:5001"); // Auth Service URL
                            var student = await authClient.GetFromJsonAsync<StudentDto>($"/api/auth/students/{studentId}");

                            studentCourseList.Add(new
                            {
                                StudentId = studentId,
                                FirstName = student?.FirstName,
                                LastName = student?.LastName,
                                CourseCode = course.CourseCode,
                                CourseId = course.CourseId
                            });
                        }
                    }
                    else
                    {
                        studentCourseList.Add(new
                        {
                            StudentId = "N/A",
                            FirstName = "N/A",
                            LastName = "N/A",
                            CourseCode = "N/A",
                            CourseId = courseId
                        });
                    }
                }
                catch (Exception ex)
                {
                    studentCourseList.Add(new
                    {
                        StudentId = "N/A",
                        FirstName = "N/A",
                        LastName = "N/A",
                        CourseCode = "N/A",
                        CourseId = courseId
                    });
                }
            }

            return Ok(studentCourseList);
        }
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

        public class GradeDto
        {
            public string StudentId { get; set; }
            public string CourseId { get; set; }
            public string Grade { get; set; }
        }

        [HttpPost("UploadGradeToDB")]
        public async Task<IActionResult> UploadGradeToDB([FromBody] GradeDto payload)
        {
            if (payload == null || string.IsNullOrEmpty(payload.StudentId)
                || string.IsNullOrEmpty(payload.CourseId) || string.IsNullOrEmpty(payload.Grade))
            {
                return BadRequest("Invalid payload");
            }

            var newGrade = new GradeModel
            {
                StudentId = payload.StudentId,
                CourseId = payload.CourseId,
                GradeValue = payload.Grade // Ensure property name matches your model
            };

            await _context.Grades.AddAsync(newGrade);
            await _context.SaveChangesAsync();

            return Ok();
        }




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
