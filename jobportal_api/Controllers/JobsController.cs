using jobportal_api.DTO;
using jobportal_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace jobportal_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public JobsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("createjob")]
        public async Task<ActionResult> CreateJobs([FromBody] JobCreateDTO jobdto)
        {
            try
            {
                //var userId = HttpContext.Session.GetString("userId");
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "User not authorised" });
                }

                var jobdetails = new Jobs
                {
                    Title = jobdto.Title,
                    Description = jobdto.Description,
                    SkillsRequired = jobdto.SkillsRequired,
                    Qualifications = jobdto.Qualifications,
                    Role = jobdto.Role,
                    Locations = jobdto.Locations,
                    Type = jobdto.Type,
                    Experience = jobdto.Experience,
                    CreatedBy = userId
                };

                _context.Jobs.Add(jobdetails);
                var result = await _context.SaveChangesAsync();

                if (result == 0)
                {
                    return BadRequest(new { success = false, message = "Error posting job" });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Job {jobdetails.JobId} posted successfully",
                    jobdetails.JobId,
                    jobdetails.Title,
                    jobdetails.Role
                });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = inner
                });
            }

        }


        [HttpGet("jobs")]
        public async Task<ActionResult> FindJobs()
        {
            try
            {
                var data = await _context.Jobs.ToListAsync();
                    return Ok(data);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }


        }

        [HttpGet("jobdetails/{jobid}")]
        public async Task<ActionResult> FindJobsByJobId([FromRoute] string jobid)
        {
            var jobexists = await _context.Jobs.FirstOrDefaultAsync(j => j.JobId == jobid);

            if (jobexists == null)
            {
                return NotFound(new { success = false, message = jobid + " doesn't exists in database" });
            }
            else
            {
                var jobs = await _context.Jobs.Where(j => j.JobId == jobid).ToListAsync();

                if (jobs != null)
                {
                    return Ok(new { success = true, message = "Details fetched successfully for job " + jobid, jobs });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Unable to get job details" });
                }
            }
        }

        [HttpPost("applyjob")]
        public async Task<ActionResult> ApplyJob([FromBody] AppliedJobs appliedjob)
        {

            //var userId = HttpContext.Session.GetString("userId");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try {

                var existingApplies = await _context.AppliedJobs
                                 .Where(a => a.JobId == appliedjob.JobId && a.UserId == userId)
                                 .ToListAsync();

                if (existingApplies.Any())
                {
                    return BadRequest( new { success = false, message = "You have already applied for this job" });
                }

                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "User not authorised" });
                }

                var apply = new AppliedJobs
                {
                    JobId = appliedjob.JobId,
                    UserId = userId,
                    NoticePeriod = appliedjob.NoticePeriod,
                    ReadyToRelocate = appliedjob.ReadyToRelocate,
                    CurrentLocation = appliedjob.CurrentLocation,
                    AppliedDate = DateOnly.FromDateTime(DateTime.Now),
                    Status=appliedjob.Status
                };

                 _context.AppliedJobs.Add(apply);
                var result = await _context.SaveChangesAsync();

                if (result == 0)
                {
                    return BadRequest(new {success=false, message="Failed to apply for job"});
                }

                return Ok(new {success = true, message = "Applied Successfully", appliedjob});
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }

        }

        [HttpGet("CheckAppliedJobs")]
        public async Task<ActionResult> CheckAppliedJobs([FromQuery] string jobid)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "User not authorized" });
                }

                var existingApplies = await _context.AppliedJobs
                    .AnyAsync(a => a.JobId == jobid && a.UserId == userId);

                if (existingApplies)
                {
                    return Ok(new { success = true, message = "You have already applied for this job" });
                }

                return Ok(new { success = false, message = "You can apply for this job" });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
        }

        [Authorize]
        [Authorize]
        [HttpGet("jobsapplied")]
        public async Task<ActionResult> GetAppliedJobs([FromQuery] string? status)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var role = User.FindFirstValue(ClaimTypes.Role);

                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Unauthorized user" });
                }

                SqlParameter userParam = (role == "Admin" || role == "Employer")
                    ? new SqlParameter("@userIdParam", DBNull.Value) 
                    : new SqlParameter("@userIdParam", userId);      

                var statusParam = new SqlParameter("@statusParam", status ?? (object)DBNull.Value);

                List<AppliedJobsDTO> appliedjobs = await _context.AppliedJobsDTO
                    .FromSqlRaw("EXEC sp_getAppliedJobs @userIdParam, @statusParam", userParam, statusParam)
                    .ToListAsync();

                if (appliedjobs == null || appliedjobs.Count == 0)
                {
                    return NotFound(new { success = false, message = "No jobs found" });
                }

                return Ok(new { success = true, appliedjobs });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
        }


        [Authorize]
        [HttpGet("interviews/{appId}")]
        public async Task<ActionResult> GetInterviewById([FromRoute] string appId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var role = User.FindFirstValue(ClaimTypes.Role);

                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Unauthorized user" });
                }

                var interviewDetails = await _context.AppliedJobs
                    .Where(a => a.ApplicationId == appId)
                    .Select(a => new
                    {
                        a.ApplicationId,
                        a.JobId,
                        a.UserId,
                        a.Status,
                        a.InterviewDate
                    })
                    .FirstOrDefaultAsync();

                if (interviewDetails == null)
                {
                    return NotFound(new { success = false, message = "No Interviews" });
                }

                if (role == "Employee" && interviewDetails.UserId != userId)
                {
                    return Unauthorized(new { success = false, message = "You are not allowed to view this interview" });
                }

                return Ok(new { success = true, interviewDetails });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
        }



        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications()
        {
            try
            {
               
                var applications = await _context.Applications
                    .FromSqlRaw("EXEC sp_getApplications")
                    .ToListAsync();

                if (applications == null || applications.Count == 0)
                {
                    return NotFound(new { success = false, message = "No Applications Found" });
                }

                var userIds = applications.Select(a => a.UserId).Distinct().ToList();

                var userProfiles = await _context.Users
                    .Where(u => userIds.Contains(u.UserId))
                    .Select(u => new UserProfileDTO
                    {
                        UserId = u.UserId,
                        Name = u.Name,
                        Email = u.Email,
                        Gender = u.Gender,
                        Contact = u.Contact,
                        Role = u.Role,
                        Bio = u.Bio,
                        Skills = u.Skills,
                        Projects = _context.Projects
                            .Where(p => p.UserId == u.UserId)
                            .Select(p => new ProjectDTO
                            {
                                ProjectID = p.ProjectID,
                                Title = p.Title,
                                Description = p.Description,
                                Skills = p.Skills
                            }).ToList(),
                        WorkExperiences = _context.WorkEx
                            .Where(w => w.UserId == u.UserId)
                            .Select(w => new WorkExDTO
                            {
                                WorkExID = w.WorkExID,
                                Company = w.Company,
                                FromDate = w.FromDate,
                                ToDate = w.ToDate
                            }).ToList()
                    })
                    .ToListAsync();

                // Merge application + profile data
                var result = applications.Select(app => new
                {
                    Application = app,
                    UserProfile = userProfiles.FirstOrDefault(p => p.UserId == app.UserId)
                });

                return Ok(new { success = true, result });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
        }

        [Authorize(Roles = "Admin,Employer")]
        [HttpPut("updatestatus")]
        public async Task<ActionResult> UpdateApplicationStatus(string applicationId, [FromBody] string status)
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role);
                var application = await _context.AppliedJobs
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

                if (application == null)
                {
                    return NotFound(new { success = false, message = "Application not found" });
                }
                application.Status = status;
                _context.AppliedJobs.Update(application);
                await _context.SaveChangesAsync();

                //Logic to send notification
                var user = application.UserId;
                var notification = new Notification
                {
                    UserId = user,
                    Message = $"Status for application {applicationId} has been updated to {status}",
                    CreatedAt = DateTime.Now
                };

                var notify = await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, application });

            }
            catch (Exception ex) {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
        }

        [HttpPost("scheduleinterview")]
        public async Task<ActionResult> ScheduleInterview([FromBody] InterviewScheduleDTO interviewSchedule)
        {
            try
            { 
                var application = await _context.AppliedJobs
                    .FirstOrDefaultAsync(a => a.ApplicationId == interviewSchedule.ApplicationId && a.UserId == interviewSchedule.UserId);
                if (application == null)
                {
                    return NotFound(new { success = false, message = "Application not found" });
                }
                application.Status = "Interview Scheduled";
                application.InterviewDate = interviewSchedule.InterviewDate;
                
                _context.AppliedJobs.Update(application);
                await _context.SaveChangesAsync();
                // Logic to send notification
                var user = application.UserId;
                var notification = new Notification
                {
                    UserId = user,
                    Message = $"Your Interview has been scheduled for {application.ApplicationId}",
                    CreatedAt = DateTime.Now
                };

                var notify = await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Interview scheduled successfully", application, notification });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
        }

    }
}
