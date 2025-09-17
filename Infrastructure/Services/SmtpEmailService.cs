using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Infrastructure.Interfaces;

namespace Infrastructure.Services;

// Implementation av IEmailService som skickar riktiga mail via SMTP.
// Om ingen SMTP är konfigurerad loggas istället ett "mock-mail" till konsolen.
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public SmtpEmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendCancellationAsync(string toEmail, string subject, string body)
    {
        // Hämta SMTP-inställningar från appsettings.json
        var host = _config["Smtp:Host"];
        var from = _config["Smtp:From"];
        var user = _config["Smtp:User"];
        var pass = _config["Smtp:Password"];
        var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;

        // Om inget SMTP är konfigurerat → mocka till konsolen
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            Console.WriteLine($"[MOCK MAIL] Till={toEmail} | Ämne={subject}\n{body}");
            await Task.CompletedTask;
            return;
        }

        // Annars: konfigurera SmtpClient och skicka på riktigt
        using var smtp = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(user, pass)
        };

        var msg = new MailMessage(from!, toEmail, subject, body) { IsBodyHtml = false };
        await smtp.SendMailAsync(msg);
    }
}
