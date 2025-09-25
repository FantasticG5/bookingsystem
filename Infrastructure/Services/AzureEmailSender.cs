using System;
using Azure.Communication.Email;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class AzureEmailSender : IEmailSender
{
    private readonly EmailClient _client;
    private readonly string _from;

    public AzureEmailSender(IConfiguration config)
    {
        var cs = config["AzureCommunicationEmail:ConnectionString"]
                 ?? throw new InvalidOperationException("Missing ACS connection string.");
        _from = config["AzureCommunicationEmail:FromAddress"]
                ?? throw new InvalidOperationException("Missing ACS from address.");
        _client = new EmailClient(cs);
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var msg = new EmailMessage(
            senderAddress: _from,
            content: new EmailContent(subject) { Html = htmlBody },
            recipients: new EmailRecipients(new List<EmailAddress> { new(to) })
        );

        await _client.SendAsync(Azure.WaitUntil.Completed, msg, ct);
    }
}
