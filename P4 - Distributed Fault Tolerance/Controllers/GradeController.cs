using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using P4___Distributed_Fault_Tolerance.Models;
using System.Diagnostics;
using System.Text;

namespace P4___Distributed_Fault_Tolerance.Controllers
{
    public class GradeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:5003/api/grades"; // Your API endpoint for grades

        public GradeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // This method fetches grades for a specific student
        private async Task<List<GradeModel>> GetGradesAsyncForStudent()
        {
            List<GradeModel> grades = new List<GradeModel>();
            var idNumber = User.Identity.Name;
            Trace.WriteLine($"IdNumber: {idNumber}"); // Debugging line to check the IdNumber

            if (string.IsNullOrEmpty(idNumber))
            {
                return grades; // or handle the case when IdNumber is null
            }

            // Create the request body
            var requestBody = new { StudentId = idNumber };
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            Trace.WriteLine($"Request Body: {jsonContent}"); // Debugging line to check the request body
            // Send a POST request to get grades
            HttpResponseMessage response = await _httpClient.PostAsync($"{_apiBaseUrl}/getGrades", content);
            Trace.WriteLine($"HTTP Status: {response.StatusCode}");
            var responseBody = await response.Content.ReadAsStringAsync();
            Trace.WriteLine($"Response Content: {responseBody}");
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
    }

}
