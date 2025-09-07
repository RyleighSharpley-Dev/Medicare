using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Medicare_Connect.Areas.Patients.Services;

public interface IEmailSender
{
	Task SendAsync(string toEmail, string subject, string htmlBody);
}

public class SmtpEmailSender : IEmailSender
{
	private readonly IConfiguration _config;

	public SmtpEmailSender(IConfiguration config)
	{
		_config = config;
	}

	public async Task SendAsync(string toEmail, string subject, string htmlBody)
	{
		var fromName = _config["Email:FromName"] ?? "Medicare Connect";
		var fromAddress = _config["Email:FromAddress"] ?? "no-reply@example.com";
		var host = _config["Email:Smtp:Host"] ?? "smtp.gmail.com";
		var port = int.TryParse(_config["Email:Smtp:Port"], out var p) ? p : 587;
		var user = _config["Email:Smtp:User"] ?? string.Empty;
		var pass = _config["Email:Smtp:Password"] ?? string.Empty;
		var enableSsl = bool.TryParse(_config["Email:Smtp:EnableSsl"], out var ssl) ? ssl : true;

		using var client = new SmtpClient(host, port)
		{
			Credentials = new NetworkCredential(user, pass),
			EnableSsl = enableSsl
		};
		using var msg = new MailMessage()
		{
			Subject = subject,
			Body = htmlBody,
			IsBodyHtml = true,
			From = new MailAddress(fromAddress, fromName)
		};
		msg.To.Add(new MailAddress(toEmail));
		await client.SendMailAsync(msg);
	}
} 