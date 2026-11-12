using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using prakt_8_trpo.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace prakt_8_trpo.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : Controller
    {
        private static TasksDbContext _context = new TasksDbContext ();
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _hasher;

        public AuthController(IPasswordHasher<User> hasher, IConfiguration configuration)
        {
            _hasher = hasher;
            _configuration = configuration;
        }


        [HttpPost("register")]
        public IActionResult PostRegister (AuthRequest request)
        {
            if (_context.Users.Any(u => u.Login == request.Login))
                return Conflict("Логин уже существует");

            var user = new User { Login = request.Login, Role = "User" };
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            _context.Users.Add(user);
            _context.SaveChanges( );
            return Ok("Пользователь зарегистрировн");

        }
        [HttpPost("login")]
        public IActionResult PostLogin (AuthRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == request.Login);
            if (user == null)
                return Unauthorized( );

            var result = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash!,
                request.Password
            );

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Неверная пара логин/пароль");  

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                access_token = token
            });
        }

        [HttpGet("me")]
        public IActionResult Me()
        { 
            return Ok(new 
            {
                Login = User.Identity?.Name,
                IsAdmin = User.IsInRole("Admin")
            });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new [ ]
            {
                new Claim(ClaimTypes.Name, user.Login!),
                new Claim(ClaimTypes.Role, user.Role!)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "ExampleApi",
                audience: "ExampleApiClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler( ).WriteToken(token);
        }

    }
}
