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
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/enrolments  (Participant only)
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrol(EnrolmentDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var ev = await _context.Events.FindAsync(dto.EventId);
            if (ev == null) return NotFound("Event not found.");

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null || category.EventId != dto.EventId)
                return BadRequest("Category does not belong to this event.");

            // Already enrolled?
            if (await _context.Enrolments.AnyAsync(e => e.EventId == dto.EventId && e.ParticipantId == userId))
                return BadRequest("You are already enrolled in this event.");

            var enrolment = new Enrolment
            {
                ParticipantId = userId,
                EventId = dto.EventId,
                CategoryId = dto.CategoryId
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();
            return Ok("Enrolled successfully.");
        }

        // GET: api/enrolments/my  (Participant views own enrolments)
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetMyEnrolments()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            return await _context.Enrolments
                .Where(e => e.ParticipantId == userId)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .ToListAsync();
        }

        // GET: api/enrolments/event/5  (Organiser views enrolments for their event)
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolmentsForEvent(int eventId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ev = await _context.Events.FindAsync(eventId);

            if (ev == null) return NotFound();
            if (ev.OrganiserId != userId) return Forbid("You do not own this event.");

            return await _context.Enrolments
                .Where(e => e.EventId == eventId)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .ToListAsync();
        }
    }

    public class EnrolmentDto
    {
        public int EventId { get; set; }
        public int CategoryId { get; set; }
    }
}