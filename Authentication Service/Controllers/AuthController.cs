using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Authentication_Service.Model;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Authentication_Service.Data;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace Authentication_Service.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;


        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpGet("prof")]
        public async Task<IActionResult> GetProf()
        {
            var profs = await _userManager.GetUsersInRoleAsync("Teacher");
            foreach (var prof in profs)
            {
                Trace.WriteLine($"Prof: {prof.Id}, {prof.FirstName}, {prof.LastName}");
            }
            var profList = profs.Select(p => new ProfModel
            {
                Id = p.Id,
                FirstName = p.FirstName,  // You may need to access a custom property here
                LastName = p.LastName     // Same for LastName, or access the profile
            }).ToList();

            return Ok(profList);

        }


        [HttpGet("students/{idNumber}")]
        public IActionResult GetStudent(string idNumber)
        {
            var student = _userManager.Users.FirstOrDefault(s => s.Id == idNumber);
            if (student == null) return NotFound();

            return Ok(new
            {
                student.IdNumber,
                student.FirstName,
                student.LastName
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.IdNumber,
                Email = model.Email,
                IdNumber = model.IdNumber,
                FirstName = model.FirstName,
                LastName = model.LastName
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return Ok(new { message = "User registered successfully" });
            }
            return BadRequest(result.Errors);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = await GenerateJwtTokenAsync(user);

                await _userManager.UpdateAsync(user);

                return Ok(new { token });
            }
            return Unauthorized(new { message = "Invalid email or password." });
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
