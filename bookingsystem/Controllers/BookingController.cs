using Infrastructure.DTOs;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace bookingsystem.Controllers;

// API-controller för bokningar.
// Här definieras endpoints som frontend kan anropa.
[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // POST: /api/booking/cancel
    // Tar emot en CancelBookingRequest från klienten och kör avbokning.
    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel([FromBody] CancelBookingRequest request)
    {
        // Validera request
        if (request is null || request.BookingId <= 0 || string.IsNullOrWhiteSpace(request.MemberEmail))
            return BadRequest("Ogiltig förfrågan.");

        // Anropa service → avboka
        var ok = await _bookingService.CancelAsync(request);

        // Returnera svar beroende på om bokningen hittades
        return ok ? Ok(new { message = "Avbokning genomförd. Bekräftelsemail skickat." })
                  : NotFound("Bokningen kunde inte hittas.");
    }
}
