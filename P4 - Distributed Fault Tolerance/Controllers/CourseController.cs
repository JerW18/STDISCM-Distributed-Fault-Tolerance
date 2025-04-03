using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using P4___Distributed_Fault_Tolerance.Models;

namespace P4___Distributed_Fault_Tolerance.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:5002/api/course";

        public CourseController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<List<Course>> GetCoursesAsync()
        {
            List<Course> courses = new List<Course>();
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/getCourses");
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

            var json = JsonConvert.SerializeObject(new { CourseId = courseId, IdNumber = idNumber });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/enrollStudent", content);

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
