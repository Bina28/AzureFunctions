using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FunctionAppAzure.Services;

public class EmailService : IEmailService
{
	private readonly IConfiguration _configuration;
	private readonly ILogger<EmailService> _logger;

	public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
	{
		_configuration = configuration;
		_logger = logger;
	}

	public async Task SendEmailAsync(string toEmail, string subject, string body)
	{
		try
		{
			// Get the secret from Azure Key Vault
			var keyVaultUri = _configuration["VaultUri"];
			
			var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
			KeyVaultSecret secret = await client.GetSecretAsync("SMTPPASSWORD");
			string smtpPassword = secret.Value;

			_logger.LogInformation($"Attempting to send email to {toEmail}");

			using var smtpClient = new SmtpClient("smtp.gmail.com")
			{
				Port = 587,
				Credentials = new NetworkCredential("dyakovabina@gmail.com", smtpPassword),
				EnableSsl = true,
				Timeout = 30000
			};

			using var mailMessage = new MailMessage
			{
				From = new MailAddress("dyakovabina@gmail.com", "Order Processing System"),
				Subject = subject,
				Body = body,
				IsBodyHtml = false
			};
			mailMessage.To.Add(toEmail);
			_logger.LogInformation($"Trying to send email to: {toEmail}");

			await smtpClient.SendMailAsync(mailMessage);
			_logger.LogInformation($"Email sent successfully to {toEmail}");
		}
		catch (Exception ex)
		{
			_logger.LogError($"Email sending failed: {ex.Message}");
			if (ex.InnerException != null)
			{
				_logger.LogError($"Inner exception: {ex.InnerException.Message}");
			}
			throw;
		}
	}
}

