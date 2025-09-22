using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

// Service för att skicka bekräftelse-email vid bokning
public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration configuration)
    {
        _config = configuration;
    }

    // Skickar bekräftelsemail när en bokning är gjord
    public async Task SendBookingConfirmationAsync(string recipientEmail, int classId, int userId)
    {
        // Hämta email-inställningar från appsettings.json
        var smtpServer = _config["Email:SmtpServer"]!;
        var smtpPort = int.Parse(_config["Email:SmtpPort"]!);
        var senderEmail = _config["Email:SenderEmail"]!;
        var senderName = _config["Email:SenderName"]!;
        var password = _config["Email:Password"]!;
        var enableSsl = bool.Parse(_config["Email:EnableSsl"]!);

        // Skapa email-meddelandet
        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = "Bokningsbekräftelse - Fantastiska G5",
            Body = $@"
            Hej!

            Din bokning är bekräftad:

            🏆 PASS-INFORMATION:
            - Pass ID: {classId}
            - Titel: [Hämtas från TrainingClasses API]
            - Datum & Tid: [Hämtas från TrainingClasses API]
            - Plats: Fantastiska G5 Gym, Stockholm
            - Instruktör: [Hämtas från TrainingClasses API]

            📝 BOKNINGSDETALJER:
            - Ditt användar-ID: {userId}
            - Bokad: {DateTime.Now:yyyy-MM-dd HH:mm}

            Tack för att du bokar hos oss!

            Med vänliga hälsningar,
            Fantastiska G5 Team
            ",
            IsBodyHtml = false
        };

        // Lägg till mottagare
        mailMessage.To.Add(recipientEmail);

        // Konfigurera SMTP-klienten (för att skicka emailet)
        using var smtpClient = new SmtpClient(smtpServer, smtpPort)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = false,
            // Hämtar lösenordet från konfigurationen (appsettings.json)
            Credentials = new NetworkCredential(senderEmail, password)
        };

        // Skicka emailet
        await smtpClient.SendMailAsync(mailMessage);
    }
}