using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using jobportal_api.Models;
using jobportal_api.DTO;

namespace jobportal_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("session")]
        public IActionResult CheckSession()
        {
            var userId = HttpContext.Session.GetString("userId");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return Unauthorized();

            return Ok(new { user.UserId, user.Name, user.Role });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDTO login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == login.Email.ToLower());

            if (user == null)
                return NotFound(new { message = "User not found" });

            var validPwd = BCrypt.Net.BCrypt.Verify(login.Password, user.Password);
            if (!validPwd)
                return Unauthorized(new { message = "Invalid password" });

            HttpContext.Session.SetString("userId", user.UserId);
            Console.WriteLine("User ID from session: " + user.UserId);

            return Ok(new { user.UserId, user.Name, user.Role });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUpUser([FromBody] UserCreateDTO user)
        {
            if (user == null)
                return BadRequest("User data is null");

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == user.Email.ToLower());

            if (emailExists)
                return BadRequest(new { message = "User with this email already exists" });

            if (!user.Email.Contains("@"))
                return BadRequest(new { message = "Please enter correct email address" });

            var hashPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

            var userData = new Users
            {
                Name = user.Name,
                Email = user.Email,
                Password = hashPassword,
                Role = user.Role,
                Gender = user.Gender,
                Contact = user.Contact,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(userData);
            await _context.SaveChangesAsync();

            return Ok(new { userData.UserId, userData.Name, userData.Role });
        }

        [HttpGet("userprofile/{userid}")]
        public async Task<IActionResult> UserProfile(string userid)
        {
            var data = await _context.Users
                .Where(u => u.UserId == userid)
                .Select(u => new
                {
                    u.UserId,
                    u.Contact,
                    u.Role,
                    u.Name,
                    u.Email,
                    u.Gender
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(new { message = $"{userid} not found" });

            return Ok(data);
        }

        [HttpPut("updatepassword/{email}")]
        public async Task<IActionResult> UpdatePassword([FromBody] LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(new { message = "Password updated successfully" });

            return BadRequest(new { message = "Failed to update password" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok();
        }
    }
}
