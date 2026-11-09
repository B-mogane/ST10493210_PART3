using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Authorize]
[Route("api/enrolments")]
public class EnrolmentsController(RaceDayDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("/api/events/{eventId:int}/enrolments"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganiserID != CurrentUserId) return Forbid();
        return Ok(await db.Enrolments.Where(e => e.Category!.EventID == eventId)
            .Select(e => new { e.EnrolmentID, e.UserID, e.CategoryID, e.Status, e.EnrolmentDate,
                Participant = e.User!.FirstName + " " + e.User.LastName, Category = e.Category!.CategoryName })
            .ToListAsync());
    }

    [HttpPost, Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(EnrolmentRequest request)
    {
        var category = await db.Categories.Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == request.CategoryId);
        if (category is null || category.Event!.Status != "Published" || category.Event.EventDate <= DateTime.UtcNow)
            return BadRequest(new { message = "Category or event is invalid or unavailable." });
        if (await db.Enrolments.AnyAsync(e => e.UserID == CurrentUserId && e.CategoryID == request.CategoryId && e.Status == "Confirmed"))
            return Conflict(new { message = "Already enrolled in this category." });
        var count = await db.Enrolments.CountAsync(e => e.CategoryID == request.CategoryId && e.Status == "Confirmed");
        if (count >= category.MaxParticipants) return BadRequest(new { message = "Category is full." });
        var enrolment = new Enrolment { UserID = CurrentUserId, CategoryID = category.CategoryID,
            EnrolmentDate = DateTime.UtcNow, Status = "Confirmed" };
        db.Enrolments.Add(enrolment);
        await db.SaveChangesAsync();
        return StatusCode(201, new { enrolment.EnrolmentID, enrolment.UserID, enrolment.CategoryID, enrolment.Status, enrolment.EnrolmentDate });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var item = await db.Enrolments.Include(e => e.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);
        if (item is null) return NotFound();
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Participant" && item.UserID != CurrentUserId) return Forbid();
        if (role == "Organiser" && item.Category!.Event!.OrganiserID != CurrentUserId) return Forbid();
        if (role is not ("Participant" or "Organiser")) return Forbid();
        item.Status = "Cancelled";
        await db.SaveChangesAsync();
        return NoContent();
    }
}
