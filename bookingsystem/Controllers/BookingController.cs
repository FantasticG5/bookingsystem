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

        public BookingController(IBookingService bookingService, IHttpClientFactory httpClientFactory)
        {
            _bookingService = bookingService;
            _seatsApi = httpClientFactory.CreateClient("TrainingClasses");
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
    }
}
