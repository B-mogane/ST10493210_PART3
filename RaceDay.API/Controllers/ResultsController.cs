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
[Route("api/results")]
public class ResultsController(RaceDayDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost, Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(ResultRequest request)
    {
        var enrolment = await db.Enrolments.Include(e => e.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentID == request.EnrolmentId);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.Category!.Event!.OrganiserID != CurrentUserId) return Forbid();
        if (enrolment.Category.Event.EventDate > DateTime.UtcNow && enrolment.Category.Event.Status != "Completed")
            return BadRequest(new { message = "Results can only be captured after the event has concluded." });
        if (await db.Results.AnyAsync(r => r.EnrolmentID == request.EnrolmentId))
            return Conflict(new { message = "A result already exists for this enrolment." });
        if (request.FinishTime < TimeSpan.Zero || (request.Position.HasValue && request.Position.Value < 1))
            return BadRequest(new { message = "Finish time must be non-negative and position must be positive." });
        var result = new Result { EnrolmentID = request.EnrolmentId, FinishTime = request.FinishTime,
            Position = request.Position, Notes = request.Notes };
        db.Results.Add(result);
        await db.SaveChangesAsync();
        return StatusCode(201, result);
    }

    [HttpGet("{enrolmentId:int}")]
    public async Task<IActionResult> GetForEnrolment(int enrolmentId)
    {
        var result = await db.Results.Include(r => r.Enrolment).ThenInclude(e => e!.Category)
            .ThenInclude(c => c!.Event).FirstOrDefaultAsync(r => r.EnrolmentID == enrolmentId);
        if (result is null) return NotFound();
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Participant" && result.Enrolment!.UserID != CurrentUserId) return Forbid();
        if (role == "Organiser" && result.Enrolment!.Category!.Event!.OrganiserID != CurrentUserId) return Forbid();
        return Ok(new { result.ResultID, result.EnrolmentID, result.FinishTime, result.Position, result.Notes });
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, UpdateResultRequest request)
    {
        var result = await db.Results.Include(r => r.Enrolment).ThenInclude(e => e!.Category)
            .ThenInclude(c => c!.Event).FirstOrDefaultAsync(r => r.ResultID == id);
        if (result is null) return NotFound();
        if (result.Enrolment!.Category!.Event!.OrganiserID != CurrentUserId) return Forbid();
        if (request.FinishTime < TimeSpan.Zero || (request.Position.HasValue && request.Position.Value < 1))
            return BadRequest(new { message = "Finish time must be non-negative and position must be positive." });
        result.FinishTime = request.FinishTime; result.Position = request.Position; result.Notes = request.Notes;
        await db.SaveChangesAsync();
        return Ok(result);
    }
}
