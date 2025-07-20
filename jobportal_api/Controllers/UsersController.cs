using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
        //[HttpGet("users")]
        //public async Task<ActionResult> GetUsers()
        //{
        //    var users = await _context.Users.ToListAsync();
        //    return Ok(users);
        //}

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser([FromBody]LoginDTO login)
        {
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower()==login.Email);

            if (user == null)
            {
                return NotFound(new { sucess=false,message = "User not found" });
            }
            
            var pwd = user.Password;
            var validPwd = BCrypt.Net.BCrypt.Verify(login.Password, pwd); 

            if (!validPwd)
            {
                return Unauthorized(new {success=false, message = "Invalid password" });
            }

            return Ok(new { success = true, user.UserId, user.Name });
        }

        [HttpPost("signup")]
        public async Task<ActionResult> SignUpUser([FromBody]UserCreateDTO user)
        {
            if (user == null) {
            return NotFound("User data is null");
            }

            bool emailExists = await _context.Users
                   .AnyAsync(u => u.Email.ToLower() == user.Email.ToLower());

            if (emailExists)
            {
                return BadRequest(new {success=false, message = "User with this email already exists" });
            }

            var hashPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

            if (!user.Email.Contains("@"))
            {
                return BadRequest(new { success=false, message = "Please enter correct email address" });
            }

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

            return Ok(new { success = true, message = userData.UserId + " Created Successfully"  });

        }
    }
}
