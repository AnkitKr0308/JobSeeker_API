using jobportal_api.DTO;
using jobportal_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

        [HttpPost("createjob")]

        public async Task<ActionResult> CreateJobs([FromBody] JobCreateDTO jobdto)
        {
            try
            {
                var userId = HttpContext.Session.GetString("userId");

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

            var userId = HttpContext.Session.GetString("userId");
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

        [HttpGet("jobsapplied")]
        public async Task<ActionResult> GetAppliedJobs()
        {
            try
            {
                var userId = HttpContext.Session.GetString("userId");

                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Unauthorized user" });
                }

                var userParam = new SqlParameter("@userIdParam", userId);
                var appliedjobs = await _context.Jobs.FromSqlRaw("EXEC sp_getAppliedJobs @userIdParam", userParam).ToListAsync();

                if (appliedjobs == null || appliedjobs.Count == 0)
                {
                    return NotFound(new { success = false, message = "You have not applied to any jobs yet" });
                }
               

                return Ok(new { success = true, appliedjobs });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { success = false, message = "Internal server error", error = errorMessage });
            }
        }

    }
}
