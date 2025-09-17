namespace Infrastructure.Interfaces;

// Interface = kontrakt. Vi definierar "vad" en e-posttjänst ska kunna göra.
public interface IEmailService
{
    // Skicka ett avbokningsmail (asynkront)
    Task SendCancellationAsync(string toEmail, string subject, string body);
}
