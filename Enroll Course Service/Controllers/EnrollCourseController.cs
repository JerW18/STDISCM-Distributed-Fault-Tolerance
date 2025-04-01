using Microsoft.AspNetCore.Mvc;

namespace Enroll_Course_Service.Controllers
{
    public class EnrollCourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
