using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
public class CategoriesController(RaceDayDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("/api/events/{eventId:int}/categories")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await db.Events.AnyAsync(e => e.EventID == eventId)) return NotFound();
        return Ok(await db.Categories.Where(c => c.EventID == eventId)
            .Select(c => new { c.CategoryID, c.EventID, c.CategoryName, c.DistanceKm, c.MaxParticipants, c.Fee })
            .ToListAsync());
    }

    [HttpPost("/api/events/{eventId:int}/categories"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int eventId, CategoryRequest request)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganiserID != CurrentUserId) return Forbid();
        if (request.DistanceKm <= 0 || request.MaxParticipants <= 0 || request.Fee < 0)
            return BadRequest(new { message = "Distance and capacity must be positive; fee cannot be negative." });
        var category = new Category { EventID = eventId, CategoryName = request.CategoryName.Trim(),
            DistanceKm = request.DistanceKm, MaxParticipants = request.MaxParticipants, Fee = request.Fee };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return StatusCode(201, category);
    }

    [HttpPut("/api/categories/{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, CategoryRequest request)
    {
        var category = await db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category is null) return NotFound();
        if (category.Event!.OrganiserID != CurrentUserId) return Forbid();
        if (request.DistanceKm <= 0 || request.MaxParticipants <= 0 || request.Fee < 0)
            return BadRequest(new { message = "Distance and capacity must be positive; fee cannot be negative." });
        category.CategoryName = request.CategoryName.Trim(); category.DistanceKm = request.DistanceKm;
        category.MaxParticipants = request.MaxParticipants; category.Fee = request.Fee;
        await db.SaveChangesAsync();
        return Ok(category);
    }

    [HttpDelete("/api/categories/{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.Include(c => c.Event).Include(c => c.Enrolments)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category is null) return NotFound();
        if (category.Event!.OrganiserID != CurrentUserId) return Forbid();
        if (category.Enrolments.Count > 0)
            return Conflict(new { message = "Cannot delete a category with enrolments." });
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
