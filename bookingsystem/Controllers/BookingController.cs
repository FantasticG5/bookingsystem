using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.DTOs;
using Infrastructure.Interfaces;
using System.Security.Claims;

namespace bookingsystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly HttpClient _seatsApi;
        private readonly IBookingService _service;

        public BookingController(IBookingService bookingService, IHttpClientFactory httpClientFactory, IBookingService service)
        {
            _bookingService = bookingService;
            _seatsApi = httpClientFactory.CreateClient("TrainingClasses");
            _service = service;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var bookings = await _service.GetByUserAsync(userId, ct);
            return Ok(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> BookClass([FromBody] CreateBookingRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            // 1) reservera plats i EventSystem
            var reserve = await _seatsApi.PostAsJsonAsync(
                $"api/trainingclasses/{req.ClassId}/seats/reserve",
                new { Seats = 1 }, ct);

            if (!reserve.IsSuccessStatusCode)
                return Conflict(new { message = "Could not reserve seat." });

                var email = User.FindFirstValue(ClaimTypes.Email);

            try
            {
                // 2) skapa bokning
                var booking = await _bookingService.BookClassAsync(userId, req.ClassId, email, ct);
                return Ok(new BookingReadDto(booking.Id, booking.ClassId, booking.CreatedAt, booking.IsCancelled));
            }
            catch (Exception ex)
            {
                // 3) rollback på seats
                await _seatsApi.PostAsJsonAsync(
                    $"api/trainingclasses/{req.ClassId}/seats/release",
                    new { Seats = 1 }, ct);
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] CancelBookingRequest req, CancellationToken ct)
        {
           var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email  = User.FindFirstValue(ClaimTypes.Email);

            var changed = await _bookingService.CancelBookingAsync(userId!, req.ClassId, email, ct);
            if (!changed)
                return Ok(new { message = "Already cancelled or not found." }); // eller 404 om du vill

            // släpp EN plats när vi faktiskt avbokade
            var release = await _seatsApi.PostAsJsonAsync(
                $"api/trainingclasses/{req.ClassId}/seats/release",
                new { Seats = 1 }, ct);

            if (!release.IsSuccessStatusCode)
                return Ok(new { message = "Avbokad, men platsen kunde inte släppas nu." });

            return Ok(new { message = "Avbokning klar." });
        }
    }
}
