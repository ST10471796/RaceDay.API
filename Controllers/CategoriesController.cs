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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/categories/event/5
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategoriesForEvent(int eventId)
        {
            return await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();
        }

        // POST: api/categories/event/5  (Organiser only)
        [HttpPost("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Category>> CreateCategory(int eventId, CategoryDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ev = await _context.Events.FindAsync(eventId);

            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserId != userId) return Forbid("You do not own this event.");

            var category = new Category
            {
                Name = dto.Name,
                EventId = eventId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // DELETE: api/categories/5  (Organiser only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();
            if (category.Event.OrganiserId != userId) return Forbid("You do not own this event.");

            if (await _context.Enrolments.AnyAsync(e => e.CategoryId == id))
                return BadRequest("Cannot delete a category that has enrolments.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok("Category deleted.");
        }
    }

    public class CategoryDto
    {
        public string Name { get; set; } = string.Empty;
    }
}