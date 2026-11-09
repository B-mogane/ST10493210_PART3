using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Authorize(Roles = "Participant")]
[Route("api/payments")]
public class PaymentsController(RaceDayDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Create(PaymentRequest request)
    {
        var enrolment = await db.Enrolments.Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.EnrolmentID == request.EnrolmentId && e.UserID == CurrentUserId);
        if (enrolment is null || enrolment.Status != "Confirmed")
            return BadRequest(new { message = "Enrolment is invalid or not owned by the current participant." });
        if (await db.Payments.AnyAsync(p => p.EnrolmentID == request.EnrolmentId))
            return BadRequest(new { message = "This enrolment already has a payment record." });
        if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            return BadRequest(new { message = "Payment method is required." });

        // This records a payment entry only. A real payment gateway must confirm payment before marking it paid.
        var payment = new Payment { EnrolmentID = enrolment.EnrolmentID, Amount = enrolment.Category!.Fee,
            PaymentMethod = request.PaymentMethod.Trim(), TransactionID = request.TransactionId, PaymentDate = DateTime.UtcNow };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return StatusCode(201, new { payment.PaymentID, payment.EnrolmentID, payment.Amount, payment.PaymentDate,
            payment.PaymentMethod, payment.TransactionID, notice = "Payment record created; no external gateway was charged." });
    }
}
