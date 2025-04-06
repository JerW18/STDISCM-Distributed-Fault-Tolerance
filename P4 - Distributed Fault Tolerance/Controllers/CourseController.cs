using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using P4___Distributed_Fault_Tolerance.Models;

namespace P4___Distributed_Fault_Tolerance.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly HttpClient _courseClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourseController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _courseClient = httpClientFactory.CreateClient("CourseApiClient");
            _httpContextAccessor = httpContextAccessor;
        }
        private string GetUserAccessToken()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst("Token")?.Value;
        }

        private async Task<List<Course>> GetCoursesAsync()
        {
            List<Course> courses = new List<Course>();

            var token = GetUserAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("API Access Token is missing."); 
                return courses; 
            }
            _courseClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await _courseClient.GetAsync("getCourses");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                courses = JsonConvert.DeserializeObject<List<Course>>(jsonData);
            }
            return courses;
        }

        public async Task<IActionResult> ViewCourses()
        {
            List<Course> courses = await GetCoursesAsync();

            return View("ViewCourses", courses);
        }

        public async Task<IActionResult> ViewEnrollCourse()
        {
            List<Course> courses = await GetCoursesAsync();

            return View("EnrollCourse", courses);
        }

        public async Task<IActionResult> EnrollStudent(int courseId)
        {
            List<Course> courses = await GetCoursesAsync();
            var idNumber = User.Identity.Name;

            var token = GetUserAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError(string.Empty, "Unable to authorize API request. Please log in again.");
                // Pass the potentially empty 'courses' list back to the view
                return View("EnrollCourse", courses ?? new List<Course>());
            }
            _courseClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(new { CourseId = courseId, IdNumber = idNumber });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _courseClient.PostAsync("enrollStudent", content);

            if (response.IsSuccessStatusCode)
            {
                courses = await GetCoursesAsync();
                TempData["SuccessMessage"] = "Enrollment successful!";
                return View("EnrollCourse", courses);
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, errorMessage);
            return View("EnrollCourse", courses);
        }
    }
}
