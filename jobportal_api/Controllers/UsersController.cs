using Azure.Core;
using jobportal_api.DTO;
using jobportal_api.Models;
using jobportal_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace jobportal_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JWTService _jwtService;

        public UsersController(AppDbContext context, JWTService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // Remove old session-based CheckSession, replace with token check if needed
        [Authorize]
        [HttpGet("session")]
        public IActionResult CheckSession()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            var token = _jwtService.GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new { user.UserId, user.Name, user.Role }
            });
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

        // Logout can be handled fully on client by deleting token, but you can keep endpoint for other logic if needed
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // No session to clear, client should delete token
            return Ok();
        }

        [Authorize]
        [HttpGet("userprofile/{userid}")]
        public async Task<IActionResult> GetUserProfile(string userid)
        {
            // Optional: Check if user is accessing own profile or authorized to access this profile
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdFromToken != userid)
            {
                // Return Forbidden or allow depending on your app's rules
                // return Forbid();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userid);

            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            //var portfolio = await _context.Portfolio.FirstOrDefaultAsync(p => p.UserId == userid);
            var projects = await _context.Projects.Where(p => p.UserId == userid).ToListAsync();
            var workExps = await _context.WorkEx.Where(w => w.UserId == userid).ToListAsync();

            var dto = new UserProfileDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Gender = user.Gender,
                Contact = user.Contact,
                Role = user.Role,
                Bio = user.Bio,
                Skills = user.Skills,

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

        [Authorize]
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
                        Bio = user.Bio,
                        Skills = user.Skills,

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
                    return NotFound(new { success = false, message = "No users found" });

                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
        }

        [Authorize]
        [HttpPost("userprofile")]
        public async Task<IActionResult> CreateUserProfile([FromBody] UserProfileDTO dto)
        {
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdFromToken != dto.UserId)
            {
                return Unauthorized("You can only create your own profile");
            }

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

        [Authorize]
        [HttpPut("userprofile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileDTO dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                    return Unauthorized("Unauthorized user");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user != null)
                {
                    user.Contact = dto.Contact;
                    user.Gender = dto.Gender;
                    user.Name = dto.Name;
                    user.Email = dto.Email;
                    user.Bio = dto.Bio;
                    user.Skills = dto.Skills;
                }

                var oldProjects = _context.Projects.Where(p => p.UserId == userId);
                _context.Projects.RemoveRange(oldProjects);
                _context.Projects.AddRange(dto.Projects.Select(p => new Projects
                {
                    UserId = userId,
                    Title = p.Title,
                    Description = p.Description,
                    Skills = p.Skills
                }));

                var oldWorkEx = _context.WorkEx.Where(w => w.UserId == userId);
                _context.WorkEx.RemoveRange(oldWorkEx);
                _context.WorkEx.AddRange(dto.WorkExperiences.Select(w => new WorkEx
                {
                    UserId = userId,
                    Company = w.Company,
                    FromDate = w.FromDate,
                    ToDate = w.ToDate
                }));

                var result = await _context.SaveChangesAsync();

                if (result == 0)
                    return BadRequest(new { success = false, message = "Unable to update profile" });

                return Ok(new { success = true, message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
        }

        //[Authorize]
        //[HttpGet("notifications")]
        //public async Task<ActionResult> GetNotifications()
        //{
        //    try
        //    {
        //        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //        if (userId == null)
        //        {
        //            return Unauthorized(new { success = false, message = "User not authorized" });
        //        }
        //        var notifications = await _context.Notifications
        //            .Where(n => n.UserId == userId && !n.IsClear)
        //            .OrderByDescending(n => n.CreatedAt)
        //            .ToListAsync();
             
        //        return Ok(new { success = true, notifications });
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorMessage = ex.InnerException?.Message ?? ex.Message;
        //        return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
        //    }
        //}

        //[Authorize]
        //[HttpPut("updatenotifications")]
        //public async Task<ActionResult> UpdateNotificationStatus([FromBody] NotificationUpdateRequest request)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    if (userId == null)
        //    {
        //        return Unauthorized(new { success = false, message = "User not authorized" });
        //    }

        //    if (request.NotificationIds == null || !request.NotificationIds.Any())
        //        return BadRequest(new { success = false, message = "No notifications specified" });

        //    var notifications = await _context.Notifications
        //        .Where(n=>request.NotificationIds.Contains(n.Id)&&n.UserId==userId)
        //        .ToListAsync();

        //    if (!notifications.Any())
        //        return NotFound(new { success = false, message = "No matching notifications found" });
        //    try
        //    {
        //        foreach (var n in notifications)
        //        {
        //            n.IsRead = request.IsRead ?? n.IsRead;
        //            n.IsClear = request.IsClear ?? n.IsClear;
        //        }
        //        var result = await _context.SaveChangesAsync();
               
        //            return Ok(new { success = true, message = "Notification updated successfully" });
               
                
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorMessage = ex.InnerException?.Message ?? ex.Message;
        //        return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
        //    }
        //}
    }
}
