using Microsoft.AspNetCore.Mvc;

namespace View_Grade_Service.Controllers
{
    public class ViewGradeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
