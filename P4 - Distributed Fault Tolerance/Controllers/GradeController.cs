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
        private readonly string _apiBaseUrl = "https://localhost:5003/api/grades";

        public GradeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
            HttpResponseMessage response = await _httpClient.PostAsync($"{_apiBaseUrl}/getGrades", content);

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
