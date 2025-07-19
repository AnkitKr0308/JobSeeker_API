using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using jobportal_api.Models;
using jobportal_api.DTO;

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
            var jobdetails = new Jobs
            {
                Title = jobdto.Title,
                Description = jobdto.Description,
                SkillsRequired = jobdto.SkillsRequired,
                Qualifications = jobdto.Qualifications,
            };

            _context.Jobs.Add(jobdetails);
            var result = await _context.SaveChangesAsync();


            if (result==0)
            {
                return BadRequest(new { success = false, message = "Error posting job"  });
            }

            return Ok(new { success=true, message = "Job Posted Successfully", jobdetails.JobId, jobdetails.Title });
        }
    }
}
