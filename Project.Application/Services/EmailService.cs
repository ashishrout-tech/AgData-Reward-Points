using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Project.Application.Services
{
    public class EmailService: IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
		}

		public async Task<bool> SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {

			var host = _configuration["Smtp:Host"];
			var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
			var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];


			var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Ashish Rout", "ashishrout655@gmail.com"));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

			await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
			await smtp.AuthenticateAsync(username, password, cancellationToken);
			await smtp.SendAsync(message, cancellationToken);
			await smtp.DisconnectAsync(true, cancellationToken);

			return true;
		}
	}
}
