using jobportal_api.DTO;
using jobportal_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                return BadRequest(new { success = false, message = "User data is null" });

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == user.Email.ToLower());

            if (emailExists)
                return BadRequest(new { success = false, message = "User with this email already exists" });

            if (!user.Email.Contains("@"))
                return BadRequest(new { success = false, message = "Please enter correct email address" });

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

            return Ok(new { success = true, userId = userData.UserId, name = userData.Name, role = userData.Role });
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


        [HttpGet("userprofile/{userid}")]
        public async Task<IActionResult> GetUserProfile(string userid)
        {
            var user = await _context.Users
       .FirstOrDefaultAsync(u => u.UserId == userid);

            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            // Get portfolio (single or null)
            var portfolio = await _context.Portfolio
                .FirstOrDefaultAsync(p => p.UserId == userid);

            // Get projects list
            var projects = await _context.Projects
                .Where(p => p.UserId == userid)
                .ToListAsync();

            // Get work experiences list
            var workExps = await _context.WorkEx
                .Where(w => w.UserId == userid)
                .ToListAsync();

            var dto = new UserProfileDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Gender = user.Gender,
                Contact = user.Contact,
                Role = user.Role,
                Bio=user.Bio,
                Skills=user.Skills,
            
                Projects = projects.Select(p => new ProjectDTO
                {
                    ProjectID = p.ProjectID,
                    Title = p.Title,
                    Description = p.Description,
                    Skills = p.Skills
                }).ToList(),
                WorkExperiences = workExps.Select(w => new WorkExDTO
                {
                    WorkExID = w.WorkExID,
                    Company = w.Company,
                    FromDate = w.FromDate,
                    ToDate = w.ToDate
                }).ToList()
            };

            return Ok(new { success = true, profiledata = dto });
        }

        [HttpGet("userprofile")]
        public async Task<IActionResult> GetAllUserProfiles()
        {
            try
            {

                var users = await _context.Users
                    .Select(user => new UserProfileDTO
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        Gender = user.Gender,
                        Contact = user.Contact,
                        Role = user.Role,
                        Bio=user.Bio,
                        Skills=user.Skills,


                        Projects = _context.Projects
                            .Where(p => p.UserId == user.UserId)
                            .Select(p => new ProjectDTO
                            {
                                ProjectID = p.ProjectID,
                                Title = p.Title,
                                Description = p.Description,
                                Skills = p.Skills
                            })
                            .ToList(),

                        WorkExperiences = _context.WorkEx
                            .Where(w => w.UserId == user.UserId)
                            .Select(w => new WorkExDTO
                            {
                                WorkExID = w.WorkExID,
                                Company = w.Company,
                                FromDate = w.FromDate,
                                ToDate = w.ToDate
                            })
                            .ToList()
                    })
                    .ToListAsync();

                if (users == null || users.Count == 0)
                {
                    return NotFound(new { success = false, message = "No users found" });
                }

                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
        }


        [HttpPost("userprofile")]
        public async Task<IActionResult> CreateUserProfile([FromBody] UserProfileDTO dto)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId);
            if (user != null)
            {
                user.Contact = dto.Contact;
                user.Gender = dto.Gender;
                user.Name = dto.Name;
                user.Email = dto.Email;
                user.Bio = dto.Bio;
                user.Skills = dto.Skills;
                user.Role = dto.Role;
            }

            var projects = dto.Projects.Select(p => new Projects
            {
                UserId = dto.UserId,
                Title = p.Title,
                Description = p.Description,
                Skills = p.Skills
            }).ToList();

            var workEx = dto.WorkExperiences.Select(w => new WorkEx
            {
                UserId = dto.UserId,
                Company = w.Company,
                FromDate = w.FromDate,
                ToDate = w.ToDate
            }).ToList();

            _context.Projects.AddRange(projects);
            _context.WorkEx.AddRange(workEx);

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Profile created successfully" });
        }


        [HttpPut("userprofile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileDTO dto)
        {
            try {
                var userid = HttpContext.Session.GetString("userId");

                if (userid == null)
                {
                    return Unauthorized("Unauthorized user");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userid);
                if (user != null)
                {
                    user.Contact = dto.Contact;
                    user.Gender = dto.Gender;
                    user.Name = dto.Name;
                    user.Email = dto.Email;
                    user.Bio = dto.Bio;
                    user.Skills = dto.Skills;
                }

                var oldProjects = _context.Projects.Where(p => p.UserId == userid);
                _context.Projects.RemoveRange(oldProjects);
                _context.Projects.AddRange(dto.Projects.Select(p => new Projects
                {
                    UserId = userid,
                    Title = p.Title,
                    Description = p.Description,
                    Skills = p.Skills
                }));

                var oldWorkEx = _context.WorkEx.Where(w => w.UserId == userid);
                _context.WorkEx.RemoveRange(oldWorkEx);
                _context.WorkEx.AddRange(dto.WorkExperiences.Select(w => new WorkEx
                {
                    UserId = userid,
                    Company = w.Company,
                    FromDate = w.FromDate,
                    ToDate = w.ToDate
                }));

                var result = await _context.SaveChangesAsync();

                if (result == 0)
                {
                    return BadRequest(new { success = false, message = "Unable to update profile" });
                }

                return Ok(new { success = true, message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
           
        }


    }

}
