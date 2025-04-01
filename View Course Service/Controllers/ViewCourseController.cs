using Microsoft.AspNetCore.Mvc;

namespace View_Course_Service.Controllers
{
    public class ViewCourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
