using Infrastructure.DTOs;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace bookingsystem.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest req, CancellationToken ct)
    {
        if (req is null || req.ClassId <= 0 || req.UserId <= 0)
                return BadRequest(new { message = "Ogiltig body: classId och userId måste vara > 0." });

        var (result, id) = await _bookingService.CreateBookingAsync(req.ClassId, req.UserId, ct);

        return result switch 
        {
            CreateBookingResult.Created => CreatedAtAction(nameof(GetById), new { id }, new CreateBookingResponse(id!.Value)),
            CreateBookingResult.AlreadyBooked => Conflict(new { message = "Du är redan bokad på detta pass."}),
            CreateBookingResult.ClassNotFound =>NotFound(new { message = "Passet finns inte."}),
            _ => Problem("Okänt fel.")

        };
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => Ok(new { bookingId = id });
}
