using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using P4___Distributed_Fault_Tolerance.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace P4___Distributed_Fault_Tolerance.Controllers
{
    public class GradeController : Controller
    {
        private readonly HttpClient _gradeClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GradeController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _gradeClient = httpClientFactory.CreateClient("GradeApiClient");
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserAccessToken()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst("Token")?.Value;
        }

        // This method fetches grades for a specific student
        private async Task<List<GradeModel>> GetGradesAsyncForStudent()
        {
            List<GradeModel> grades = new List<GradeModel>();

            var token = GetUserAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("API Access Token is missing.");
                return grades;
            }
            _gradeClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var idNumber = User.Identity.Name;

            if (string.IsNullOrEmpty(idNumber))
            {
                return grades;
            }

            // Create the request body
            var requestBody = new { IdNumber = idNumber };
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Send a POST request to get grades
            try
            {
                HttpResponseMessage response = await _gradeClient.PostAsync("getGrades", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    grades = JsonConvert.DeserializeObject<List<GradeModel>>(jsonData);
                }
                if (response.IsSuccessStatusCode == false && response.Content.ReadAsStringAsync().Result.Contains("No grades found for this student."))
                {
                    ModelState.AddModelError("", "No grades found for this student.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "The grade service is down. Please try again later.");
            }
            return grades;
        }

        // Action to view grades for a student (current user)
        public async Task<IActionResult> ViewGrades()
        {
            List<GradeModel> grades = await GetGradesAsyncForStudent();
            return View("ViewGrades", grades);
        }

        // Action to view grades for all students
        private async Task<List<GradeModel>> GetGradesAsyncForAllStudents()
        {
            List<GradeModel> grades = new List<GradeModel>();

            var token = GetUserAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("API Access Token is missing.");
                return grades;
            }
            _gradeClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var idNumber = User.Identity.Name;

            if (string.IsNullOrEmpty(idNumber))
            {
                return grades;
            }

            var requestBody = new { IdNumber = idNumber };
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            try { 
                HttpResponseMessage response = await _gradeClient.PostAsync("getAllGrades", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    grades = JsonConvert.DeserializeObject<List<GradeModel>>(jsonData);
                }
                else
                {
                    if (response.Content.ReadAsStringAsync().Result.Contains("No grades found."))
                    {
                        ModelState.AddModelError("", "No grades found.");
                    }
                    if (response.Content.ReadAsStringAsync().Result.Contains("No courses found for the specified professor."))
                    {
                        ModelState.AddModelError("", "No courses found for the specified professor.");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "The grade service is down. Please try again later.");
            }
            return grades;
        }

        // Action to view all grades for all students (for the teacher)
        public async Task<IActionResult> ViewAllGrades()
        {
            List<GradeModel> grades = await GetGradesAsyncForAllStudents();
            return View("ViewAllGrades", grades); 
        }

        // View grades of students for updating
        public async Task<IActionResult> ViewAllGradesUpdate()
        {
            List<GradeModel> grades = new List<GradeModel>();

            var token = GetUserAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("API Access Token is missing.");
                return View("UploadGrades", grades);
            }
            _gradeClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var idNumber = User.Identity.Name;

            if (string.IsNullOrEmpty(idNumber))
            {
                return View("UploadGrades", grades);
            }

            var requestBody = new { IdNumber = idNumber };
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            try { 
                HttpResponseMessage response = await _gradeClient.PostAsync("getAllGradesProf", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    grades = JsonConvert.DeserializeObject<List<GradeModel>>(jsonData);
                }
                else
                {
                    if (response.Content.ReadAsStringAsync().Result.Contains("The course service is down. Please try again later."))
                    {
                        ModelState.AddModelError("", "The course service is down. Please try again later.");
                    }
                    if (response.Content.ReadAsStringAsync().Result.Contains("The authentication service is down. Please try again later."))
                    {
                        ModelState.AddModelError("", "The authentication service is down. Please try again later.");
                    }
                    if (response.Content.ReadAsStringAsync().Result.Contains("No courses found for the specified professor."))
                    {
                        ModelState.AddModelError("", "No courses found for the specified professor.");
                    }
                    if (response.Content.ReadAsStringAsync().Result.Contains("No students to be graded."))
                    {
                        ModelState.AddModelError("", "No students to be graded.");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "The grade service is down. Please try again later.");
            }


            return View("UploadGrades", grades);
        }

        // Action to update grades
        public async Task<IActionResult> UploadGradeToDB(string StudentId, string LastName, string FirstName, string CourseId, string CourseCode, string Grade, int Units)
        {
            var idNumber = User.Identity.Name;
            if (string.IsNullOrEmpty(StudentId) || string.IsNullOrEmpty(CourseId) || string.IsNullOrEmpty(Grade) || string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(CourseCode) || Units <= 0)
            {
                TempData["Error"] = "Missing required fields.";
                return RedirectToAction("ViewAllGradesUpdate");
            }

            var token = GetUserAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("API Access Token is missing.");
                return View("ViewAllGradesUpdate");
            }
            _gradeClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var gradePayload = new
            {
                StudentId = StudentId,
                LastName = LastName,
                FirstName = FirstName,
                CourseId = CourseId,
                CourseCode = CourseCode,
                Grade = Grade,
                Units = Units,
                ProfId = idNumber
            };

            var jsonContent = JsonConvert.SerializeObject(gradePayload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try { 
                HttpResponseMessage response = await _gradeClient.PostAsync("UploadGradeToDB", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Grade uploaded successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to upload grade.";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "The grade service is down. Please try again later.");
            }

            return RedirectToAction("ViewAllGradesUpdate");
        }

    }

}
