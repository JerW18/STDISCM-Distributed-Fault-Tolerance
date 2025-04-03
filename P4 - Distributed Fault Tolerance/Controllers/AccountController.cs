using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using P4___Distributed_Fault_Tolerance.Models;

namespace P4___Distributed_Fault_Tolerance.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly string _apiBaseUrl = "https://localhost:5001/api/auth";

        public AccountController(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult LoginView() => View();
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/login", content);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(result.Token);

                var idNumberClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "IdNumber");
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, idNumberClaim.Value),
                    new("Token", result.Token),
                    new(ClaimTypes.Role, roleClaim.Value)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "login");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View(model);
            }

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/register", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            var errorResponse = await response.Content.ReadAsStringAsync();

            try
            {
                var errorList = JsonConvert.DeserializeObject<List<ApiError>>(errorResponse);
                if (errorList != null)
                {
                    foreach (var error in errorList)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "An unexpected error occurred.");
                }
            }
            catch (JsonException)
            {
                ModelState.AddModelError("", "An error occurred while processing the response.");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
