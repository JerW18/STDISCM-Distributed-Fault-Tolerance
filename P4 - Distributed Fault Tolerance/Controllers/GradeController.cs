using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using P4___Distributed_Fault_Tolerance.Models;
using System.Diagnostics;
using System.Text;

namespace P4___Distributed_Fault_Tolerance.Controllers
{
    public class GradeController : Controller
    {
        private readonly HttpClient _gradeClient;

        public GradeController(IHttpClientFactory httpClientFactory)
        {
            _gradeClient = httpClientFactory.CreateClient("GradeApiClient");
        }

        // This method fetches grades for a specific student
        private async Task<List<GradeModel>> GetGradesAsyncForStudent()
        {
            List<GradeModel> grades = new List<GradeModel>();
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
            HttpResponseMessage response = await _gradeClient.PostAsync("getGrades", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                grades = JsonConvert.DeserializeObject<List<GradeModel>>(jsonData);
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
            var idNumber = User.Identity.Name;

            if (string.IsNullOrEmpty(idNumber))
            {
                return grades;
            }

            var requestBody = new { IdNumber = idNumber };
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _gradeClient.PostAsync("getAllGrades", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                grades = JsonConvert.DeserializeObject<List<GradeModel>>(jsonData);
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
            var idNumber = User.Identity.Name;

            if (string.IsNullOrEmpty(idNumber))
            {
                return View("UploadGrades", grades);
            }

            // Create the request body
            var requestBody = new { IdNumber = idNumber };
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _gradeClient.PostAsync("getAllGradesProf", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                grades = JsonConvert.DeserializeObject<List<GradeModel>>(jsonData);
            }

            return View("UploadGrades", grades);
        }

        // Action to update grades
        public async Task<IActionResult> UploadGradeToDB(string StudentId, string CourseId, string Grade)
        {
            if (string.IsNullOrEmpty(StudentId) || string.IsNullOrEmpty(CourseId) || string.IsNullOrEmpty(Grade))
            {
                // Optionally show error or redirect back with message
                TempData["Error"] = "Missing required fields.";
                Trace.WriteLine("Missing required fields.");
                return RedirectToAction("ViewAllGradesUpdate");
            }

            // Create the request payload
            var gradePayload = new
            {
                StudentId = StudentId,
                CourseId = CourseId,
                Grade = Grade
            };

            var jsonContent = JsonConvert.SerializeObject(gradePayload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Send POST request to your API endpoint
            HttpResponseMessage response = await _gradeClient.PostAsync("UploadGradeToDB", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Grade uploaded successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to upload grade.";
            }

            // Redirect back to the view
            return RedirectToAction("ViewAllGradesUpdate");
        }

    }

}
