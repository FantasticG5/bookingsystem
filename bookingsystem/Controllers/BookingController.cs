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
        private readonly IEventHandler<BookingCreatedEvent> _bookingCreatedHandler;

        public BookingController(IBookingService bookingService, IHttpClientFactory httpClientFactory, IEventHandler<BookingCreatedEvent> bookingCreatedHandler)
        {
            _bookingService = bookingService;
            _seatsApi = httpClientFactory.CreateClient("TrainingClasses");
            _bookingCreatedHandler = bookingCreatedHandler;
        }

        [HttpPost]
        public async Task<IActionResult> BookClass([FromBody] BookingDto dto, CancellationToken ct)
        {
            //var reserveResponse = await _seatsApi.PostAsJsonAsync(
            //    $"api/trainingclasses/{dto.ClassId}/seats/reserve",
            //    new { Seats = 1 }, ct);

            //if (!reserveResponse.IsSuccessStatusCode)
            //    return Conflict(new { message = "Could not reserve seat." });

            try
            {
                var booking = await _bookingService.BookClassAsync(dto);
                
                // Publicera BookingCreated event för email-bekräftelse
                var bookingCreatedEvent = new BookingCreatedEvent
                {
                    BookingId = booking.Id,
                    ClassId = booking.ClassId,
                    UserId = booking.UserId,
                    CreatedAt = booking.CreatedAt
                };
                
                await _bookingCreatedHandler.HandleAsync(bookingCreatedEvent);
                
                return Ok(booking);
            }
            catch (Exception ex)
            {
                //await _seatsApi.PostAsJsonAsync(
                //    $"api/trainingclasses/{dto.ClassId}/seats/release",
                //    new { Seats = 1 }, ct);

                return Conflict(new { message = ex.Message });
            }
        }
    }
}
