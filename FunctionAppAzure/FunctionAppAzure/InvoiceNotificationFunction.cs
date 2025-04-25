using FunctionAppAzure.Models;
using FunctionAppAzure.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace FunctionAppAzure
{
	public class InvoiceNotificationFunction
	{
		private readonly IEmailService _emailService;

		public InvoiceNotificationFunction(	IEmailService emailService)
		{
			_emailService = emailService;		
		}

		[Function("InvoiceNotificationFunction")]
		public async Task Run(
		[BlobTrigger("invoices/{name}", Connection = "BlobConnection")] Stream invoiceBlob,
		string name, FunctionContext context)
		{
			var logger = context.GetLogger("InvoiceNotificationFunction");

			logger.LogInformation($"Processing new invoice: {name}");

				if (invoiceBlob == null || invoiceBlob.Length == 0)
				{
					logger.LogWarning($"Blob {name} is empty or not accessible");
					return;
				}

				string invoiceContent;
				try
				{
					invoiceBlob.Position = 0; // Reset stream to the beginning
					invoiceContent = await ReadTextFromBlobAsync(invoiceBlob);
				}
				catch (Exception ex)
				{
					logger.LogError($"Error reading blob {name}: {ex.Message}");
					return;
				}

				// Extract customer email
				string customerEmail;
				try
				{
					customerEmail = ExtractEmailFromInvoice(invoiceContent);
				}
				catch (FormatException ex)
				{
					logger.LogError($"Email extraction failed: {ex.Message}");
					return;
				}

				if (string.IsNullOrEmpty(customerEmail))
				{
					logger.LogError("No customer email found in invoice");
					return;
				}

				// Send email
				try
				{
					await _emailService.SendEmailAsync(customerEmail, $"Invoice: {name}", invoiceContent);					
				}
				catch (Exception ex)
				{
					logger.LogError($"Email sending failed for {customerEmail}: {ex.Message}");
				}
			}
			
		
		private static async Task<string> ReadTextFromBlobAsync(Stream blobStream)
		{
			using var reader = new StreamReader(blobStream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
			return await reader.ReadToEndAsync();
		}

		private string ExtractEmailFromInvoice(string content)
		{
			// Try to find email in plain text format
			var emailMatch = Regex.Match(content, @"Email:\s*([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})", RegexOptions.IgnoreCase);
			if (emailMatch.Success)
			{
				return emailMatch.Groups[1].Value.Trim();
			}

			// Try to parse as JSON and extract the email
			try
			{
				var invoice = JsonConvert.DeserializeObject<Order>(content);
				return invoice?.CustomerEmail?.Trim();
			}
			catch (JsonException) { }

			throw new FormatException("Could not extract email from invoice content");
		}
	}
}