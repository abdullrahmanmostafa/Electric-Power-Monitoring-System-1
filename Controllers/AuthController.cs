using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Electric_Power_Monitoring_System.Models;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Helpers;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
                return BadRequest(new AuthResponseDto { Message = "Email already registered. Please login." });

            // Get or generate user identifier from header
            var userIdentifier = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userIdentifier))
                userIdentifier = Guid.NewGuid().ToString();

            // Ensure user identifier is unique
            var existingIdentifier = await _context.Users.FirstOrDefaultAsync(u => u.UserIdentifier == userIdentifier);
            if (existingIdentifier != null)
                return BadRequest(new AuthResponseDto { Message = "User identifier already exists. Please generate a new one." });

            var user = new User
            {
                UserIdentifier = userIdentifier,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = PasswordHelper.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                UserIdentifier = user.UserIdentifier,
                Message = "Registration successful"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized(new AuthResponseDto { Message = "Invalid email or password" });

            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new AuthResponseDto { Message = "Invalid email or password" });

            // Fetch hubs linked to this user
            var hubSerials = await _context.Hubs
                .Where(h => h.UserId == user.UserIdentifier)
                .Select(h => h.Serial)
                .ToListAsync();

            return Ok(new AuthResponseDto
            {
                UserIdentifier = user.UserIdentifier,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                HubSerials = hubSerials,
                Message = "Login successful"
            });
        }
    }
}