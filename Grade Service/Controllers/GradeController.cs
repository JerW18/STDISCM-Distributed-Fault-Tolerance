using Grade_Service.Data;
using Grade_Service.Models;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class GradeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string _authServiceUrl = "http://localhost:8001";
        private readonly string _courseServiceUrl = "http://localhost:8002";

        public GradeController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateServiceClient(string baseUrl)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
            }
            return client;
        }

        [HttpPost("getAllGradesProf")]
        public async Task<IActionResult> GetAllGradesAsyncProf([FromBody] ViewGradeRequest viewGradeRequest)
        {
            if (string.IsNullOrEmpty(viewGradeRequest.IdNumber))
            {
                return BadRequest(new { message = "Professor ID is required." });
            }

            using var profClient = CreateServiceClient(_courseServiceUrl);
            List<int> courseIdsTheProfTeaches = null;

            try
            {
                courseIdsTheProfTeaches = await profClient.GetFromJsonAsync<List<int>>($"/api/course/profclass/{viewGradeRequest.IdNumber}");
                if (courseIdsTheProfTeaches == null || !courseIdsTheProfTeaches.Any())
                {
                    return NotFound(new { message = "No courses found for the specified professor." });
                }
            }
            catch (Exception)
            {
                return NotFound(new { message = "The course service is down. Please try again later." });
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

                            try
                            {
                                using var authClient = CreateServiceClient(_authServiceUrl);
                                var student = await authClient.GetFromJsonAsync<StudentDto>($"/api/auth/students/{studentId}");

                                studentCourseList.Add(new
                                {
                                    StudentId = studentId,
                                    FirstName = student?.FirstName ?? "N/A",
                                    LastName = student?.LastName ?? "N/A",
                                    CourseCode = course.CourseCode,
                                    CourseId = course.CourseId
                                });
                            }
                            catch (Exception ex)
                            {
                                return NotFound(new { message = "The authentication service is down. Please try again later." });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return NotFound(new { message = "The course service is down. Please try again later." });
                }
            }

            if (studentCourseList == null || !studentCourseList.Any())
            {
                return NotFound(new { message = "No students to be graded." });
            }

            return Ok(studentCourseList);
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

            var grades = await _context.Grades
                .Where(g => g.StudentId == viewGradeRequest.IdNumber)
                .ToListAsync();

            if (!grades.Any())
            {
                return NotFound(new { message = "No grades found for this student." });
            }

            using var courseClient = CreateServiceClient(_courseServiceUrl);
            var results = new List<object>();

            foreach (var grade in grades)
            {
                try
                {
                    // Get course details for EACH grade
                    var course = await courseClient.GetFromJsonAsync<CourseDto>($"/api/course/coursename/{grade.CourseId}");

                    results.Add(new
                    {
                        grade.GradeId,
                        grade.CourseId,
                        grade.GradeValue,
                        CourseName = course?.CourseName,
                        CourseCode = course?.CourseCode,
                        Units = course?.Units
                    });
                }
                catch (Exception ex)
                {
                        return NotFound(new { message = "The course service is down. Please try again later." });
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
            List<GradeModel> grades;
            try
            {
                using var profClient = CreateServiceClient(_courseServiceUrl);
                // Get the list of course IDs the professor teaches from the API
                var courseIdsTheProfTeaches = await profClient.GetFromJsonAsync<List<int>>($"/api/course/profclass/{viewGradeRequest.IdNumber}");

                if (courseIdsTheProfTeaches == null || !courseIdsTheProfTeaches.Any())
                {
                    return NotFound(new { message = "No courses found for the specified professor." });
                }

                var courseIdStrings = courseIdsTheProfTeaches.Select(id => id.ToString()).ToList();

                grades = await _context.Grades
                    .Where(g => courseIdStrings.Contains(g.CourseId))
                    .ToListAsync();

                if (!grades.Any())
                {
                    return NotFound(new { message = "No grades found." });
                }
            }
            catch
            {
                // Fallback: Get all grades if the course service is unavailable
                grades = await _context.Grades.ToListAsync();
            }
            var results = new List<object>();

            foreach (var grade in grades)
            {
                string courseCode = "N/A";
                string courseName = "N/A";
                string firstName = "N/A";
                string lastName = "N/A";

                try
                {
                    using var authClient = CreateServiceClient(_authServiceUrl);
                    var student = await authClient.GetFromJsonAsync<StudentDto>($"/api/auth/students/{grade.StudentId}");
                    if (student != null)
                    {
                        firstName = student.FirstName;
                        lastName = student.LastName;
                    }
                }
                catch
                {
                    return NotFound(new { message = "The authentication service is down. Please try again later." });
                }

                try
                {
                    using var courseClient = CreateServiceClient(_courseServiceUrl);
                    var course = await courseClient.GetFromJsonAsync<CourseDto>($"/api/course/coursename/{grade.CourseId}");
                    if (course != null)
                    {
                        courseCode = course.CourseCode;
                        courseName = course.CourseName;
                    }
                }
                catch
                {
                    return NotFound(new { message = "The course service is down. Please try again later." });
                }

                results.Add(new
                {
                    grade.GradeId,
                    grade.StudentId,
                    grade.CourseId,
                    CourseCode = courseCode,
                    CourseName = courseName,
                    grade.GradeValue,
                    FirstName = firstName,
                    LastName = lastName
                });
            }

            return Ok(results);
        }
    }
}
