using Microsoft.AspNetCore.Mvc;

namespace Upload_Grade_Service.Controllers
{
    public class UploadGradeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
