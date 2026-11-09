using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(RaceDayDbContext db) : ControllerBase
{
    private readonly PasswordHasher<User> _hasher = new();
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await db.Users.FindAsync(CurrentUserId);
        return user is null ? NotFound() : Ok(new UserResponse(user.UserID, user.FirstName, user.LastName, user.Email, user.Role, user.ProfilePictureName));
    }

    [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateProfileRequest request)
    {
        var user = await db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
        if (request.ProfilePictureName is not null)
            user.ProfilePictureName = request.ProfilePictureName;
        await db.SaveChangesAsync();
        return Ok(new UserResponse(user.UserID, user.FirstName, user.LastName, user.Email, user.Role, user.ProfilePictureName));
    }

    [HttpGet("me/enrolments"), Authorize(Roles = "Participant")]
    public async Task<IActionResult> MyEnrolments()
    {
        var items = await db.Enrolments.Where(x => x.UserID == CurrentUserId)
            .Include(x => x.Category).ThenInclude(x => x!.Event)
            .Select(x => new { x.EnrolmentID, x.Status, x.EnrolmentDate, Category = x.Category!.CategoryName,
                Event = x.Category!.Event!.EventName, x.Category.DistanceKm })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("me/results"), Authorize(Roles = "Participant")]
    public async Task<IActionResult> MyResults()
    {
        var items = await db.Results.Where(r => r.Enrolment!.UserID == CurrentUserId)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .Select(r => new { r.ResultID, r.FinishTime, r.Position, r.Notes,
                Event = r.Enrolment!.Category!.Event!.EventName, EventDate = r.Enrolment.Category.Event.EventDate, Category = r.Enrolment.Category.CategoryName, TotalFinishers = db.Results.Count(other => other.Enrolment!.Category!.EventID == r.Enrolment.Category.EventID && other.Position != null) })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("me/payments"), Authorize(Roles = "Participant")]
    public async Task<IActionResult> MyPayments()
    {
        var items = await db.Payments.Where(p => p.Enrolment!.UserID == CurrentUserId)
            .Select(p => new { p.PaymentID, p.EnrolmentID, p.Amount, p.PaymentDate, p.PaymentMethod, p.TransactionID })
            .ToListAsync();
        return Ok(items);
    }
}
