using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // PUT: api/results/5  (Organiser only)
        [HttpPut("{enrolmentId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> SetResult(int enrolmentId, ResultDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.Id == enrolmentId);

            if (enrolment == null) return NotFound();
            if (enrolment.Event.OrganiserId != userId) return Forbid("You do not own this event.");

            enrolment.FinishTime = dto.FinishTime;
            enrolment.Position = dto.Position;

            await _context.SaveChangesAsync();
            return Ok("Result recorded.");
        }

        // GET: api/results/my  (Participant views own results)
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyResults()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var results = await _context.Enrolments
                .Where(e => e.ParticipantId == userId && e.FinishTime != null)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Select(e => new
                {
                    EventName = e.Event.Name,
                    Category = e.Category.Name,
                    FinishTime = e.FinishTime,
                    Position = e.Position
                })
                .ToListAsync();

            return Ok(results);
        }
    }

    public class ResultDto
    {
        public string FinishTime { get; set; } = string.Empty;
        public int Position { get; set; }
    }
}