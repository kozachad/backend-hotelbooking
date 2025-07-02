using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HotelBookingSystem.Data;
using HotelBookingSystem.Entities;
using HotelBookingSystem.Helpers;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HotelBookingSystem.DTOs;

namespace HotelBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthController(HotelDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
                return BadRequest("User already exists.");

            user.UserId = Guid.NewGuid();
            user.PasswordHash = HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginUser)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == loginUser.Email);
            if (user == null || !VerifyPassword(loginUser.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var token = _jwtHelper.GenerateJwtToken(user);
            return Ok(new { token });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var hashOfInput = HashPassword(inputPassword);
            return hashOfInput == storedHash;
        }
    }
}
