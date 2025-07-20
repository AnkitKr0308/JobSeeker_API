using jobportal_api.DTO;
using jobportal_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

            return Ok(new { success=true, message ="Job "+  jobdetails.JobId+ " posted successfully", jobdetails.JobId, jobdetails.Title });
        }
        [HttpGet("jobs")]
        public async Task<ActionResult> FindJobs(string query="")
        {
            
            if (query=="")
            {
                 var data = await _context.Jobs.ToListAsync();
                return Ok(data);
            }
            else
            {
                var param = new SqlParameter("@queryParam", query);
                var data = await _context.Jobs.FromSqlRaw("EXEC sp_queryJobs @queryParam", param).ToListAsync();
                return Ok(data);
            }

           
        }
    }
}
