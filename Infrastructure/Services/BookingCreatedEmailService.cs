using Infrastructure.DTOs;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

// Event handler som skickar bekräftelsemail när BookingCreated event tas emot
public class BookingCreatedEmailService : IEventHandler<BookingCreatedEvent>
{
    private readonly EmailService _emailService;
    private readonly ILogger<BookingCreatedEmailService> _logger;

    public BookingCreatedEmailService(EmailService emailService, ILogger<BookingCreatedEmailService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(BookingCreatedEvent eventData)
    {
        try
        {
            _logger.LogInformation("Skickar bekräftelsemail för bokning {BookingId} till användare {UserId}", 
                eventData.BookingId, eventData.UserId);

            await _emailService.SendBookingConfirmationAsync(
                eventData.UserEmail,
                eventData.ClassId,
                eventData.UserId
            );

            _logger.LogInformation("Bekräftelsemail skickat för bokning {BookingId}", eventData.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Misslyckades att skicka bekräftelsemail för bokning {BookingId}", eventData.BookingId);
            // Vi kastar inte vidare - email-fel ska inte krascha systemet
        }
    }
}