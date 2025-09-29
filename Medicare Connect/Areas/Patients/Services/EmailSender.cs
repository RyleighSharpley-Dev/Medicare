using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Medicare_Connect.Areas.Patients.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public SmtpEmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var fromName = _config["Email:FromName"] ?? "Medicare Connect";
        var fromAddress = _config["Email:FromAddress"] ?? "no-reply@example.com";
        var host = _config["Email:Smtp:Host"] ?? "smtp.gmail.com";
        var port = int.TryParse(_config["Email:Smtp:Port"], out var p) ? p : 587;
        var user = _config["Email:Smtp:User"] ?? string.Empty;
        var pass = _config["Email:Smtp:Password"] ?? string.Empty;


        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        using var msg = new MailMessage()
        {
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true,
            From = new MailAddress(fromAddress, fromName)
        };
        msg.To.Add(new MailAddress(email));
        await client.SendMailAsync(msg);
    }
}
