using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.DTOs;
using Infrastructure.Interfaces;

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

        [HttpPost]
        public async Task<IActionResult> BookClass([FromBody] BookingDto dto, CancellationToken ct)
        {
            var reserveResponse = await _seatsApi.PostAsJsonAsync(
                $"api/trainingclasses/{dto.ClassId}/seats/reserve",
                new { Seats = 1 }, ct);

            if (!reserveResponse.IsSuccessStatusCode)
                return Conflict(new { message = "Could not reserve seat." });

            try
            {
                var booking = await _bookingService.BookClassAsync(dto);
                return Ok(booking);
            }
            catch (Exception ex)
            {
                await _seatsApi.PostAsJsonAsync(
                    $"api/trainingclasses/{dto.ClassId}/seats/release",
                    new { Seats = 1 }, ct);

                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] CancelBookingDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { error = "Email krävs." });

            // 1) Avboka i vår DB + skicka mail (din service gör redan båda)
            var cancelled = await _service.CancelBookingAsync(dto);
            if (!cancelled) return NotFound(new { error = "Ingen sådan bokning hittades." });

            // 2) Släpp plats i eventsystemet (1 plats)
            var releaseRes = await _seatsApi.PostAsJsonAsync(
                $"api/trainingclasses/{dto.ClassId}/seats/release",
                new { Seats = 1 }, ct);

            // Policy: välj “best effort” eller “strikt”
            if (!releaseRes.IsSuccessStatusCode)
            {
                // Best effort: returnera 200 men meddela att release misslyckades
                // (alternativ: returnera Conflict/BadGateway om du vill vara strikt)
                return Ok(new { message = "Avbokad, men kunde inte släppa platsen i klasslistan just nu." });
            }

            return Ok(new { message = "Avbokning klar och bekräftelse skickad." });
        }
    }
}
