using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
namespace RaceDay.API.Controllers;
[ApiController]
[Route("api/events")]
public class EventsController(RaceDayDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetUpcoming()
    {
        var items = await db.Events.Where(e => e.EventDate >= DateTime.UtcNow && e.Status == "Published")
            .OrderBy(e => e.EventDate).Select(e => new { e.EventID,e.EventName,e.Description,e.EventDate,e.Location,e.RouteInfo,e.EntryFee,e.Status,e.DistanceKm,e.EventType,e.BannerImageName,
                EnrolmentCount = e.Categories.SelectMany(c => c.Enrolments).Count(x => x.Status == "Confirmed") })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("mine"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Mine()
    {
        var items = await db.Events.Where(e => e.OrganiserID == CurrentUserId).OrderBy(e => e.EventDate)
            .Select(e => new { e.EventID,e.EventName,e.Description,e.EventDate,e.Location,e.RouteInfo,e.EntryFee,e.Status,e.DistanceKm,e.EventType,e.BannerImageName,
                EnrolmentCount = e.Categories.SelectMany(c => c.Enrolments).Count(x => x.Status == "Confirmed") })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await db.Events.Where(e => e.EventID == id)
            .Select(e => new { e.EventID,e.EventName,e.Description,e.EventDate,e.Location,e.RouteInfo,e.EntryFee,e.Status,e.DistanceKm,e.EventType,e.BannerImageName,
                Categories = e.Categories.Select(c => new { c.CategoryID,c.EventID,c.CategoryName,c.DistanceKm,c.MaxParticipants,c.Fee }) })
            .SingleOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost, Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(EventRequest request)
    {
        if (request.EventDate <= DateTime.UtcNow || request.EntryFee < 0 || request.DistanceKm <= 0 ||
            request.Status is not ("Draft" or "Published" or "Cancelled" or "Completed") ||
            request.EventType is not ("Run" or "Walk" or "Cycle"))
            return BadRequest(new { message = "Check event date, fee, distance, event type and status." });
        var item = new Event { OrganiserID = CurrentUserId,EventName = request.EventName.Trim(),Description = request.Description,
            EventDate = request.EventDate,Location = request.Location.Trim(),RouteInfo = request.RouteInfo,EntryFee = request.EntryFee,
            Status = request.Status,DistanceKm = request.DistanceKm,EventType = request.EventType,BannerImageName = request.BannerImageName,CreatedAt = DateTime.UtcNow };
        db.Events.Add(item); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById),new { id = item.EventID },item);
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id,EventRequest request)
    {
        var item = await db.Events.FindAsync(id);
        if (item is null) return NotFound();
        if (item.OrganiserID != CurrentUserId) return Forbid();
        if (request.EntryFee < 0 || request.DistanceKm <= 0 || request.Status is not ("Draft" or "Published" or "Cancelled" or "Completed") || request.EventType is not ("Run" or "Walk" or "Cycle"))
            return BadRequest(new { message = "Check event fee, distance, event type and status." });
        item.EventName = request.EventName.Trim(); item.Description = request.Description; item.EventDate = request.EventDate;
        item.Location = request.Location.Trim(); item.RouteInfo = request.RouteInfo; item.EntryFee = request.EntryFee;
        item.Status = request.Status; item.DistanceKm = request.DistanceKm; item.EventType = request.EventType;
        if (request.BannerImageName is not null) item.BannerImageName = request.BannerImageName;
        await db.SaveChangesAsync(); return Ok(item);
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Events.Include(e => e.Categories).ThenInclude(c => c.Enrolments).FirstOrDefaultAsync(e => e.EventID == id);
        if (item is null) return NotFound();
        if (item.OrganiserID != CurrentUserId) return Forbid();
        if (item.Categories.Any(c => c.Enrolments.Any()))
            return Conflict(new { message = "Cannot delete an event with enrolments. Cancel the event instead." });
        db.Events.Remove(item); await db.SaveChangesAsync(); return NoContent();
    }
}
