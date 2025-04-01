using Microsoft.AspNetCore.Mvc;

namespace Authentication_Service.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        [HttpGet("status")]
        public IActionResult GetStatus() => Ok("Auth Service is running");
    }
}
