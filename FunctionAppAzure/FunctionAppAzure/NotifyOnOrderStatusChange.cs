using Azure.Messaging.EventGrid;
using FunctionAppAzure.Models;
using FunctionAppAzure.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FunctionAppAzure;

public class NotifyOnOrderStatusChange
{
	private readonly ILogger _logger;
	private readonly IEmailService _emailService;

	public NotifyOnOrderStatusChange(
		ILoggerFactory loggerFactory,
		IEmailService emailService)
	{
		_logger = loggerFactory.CreateLogger<NotifyOnOrderStatusChange>();
		_emailService = emailService;
	}


	[Function(nameof(NotifyOnOrderStatusChange))]
	public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, FunctionContext context)
	{
		var logger = context.GetLogger("ProcessOrder");
		logger.LogInformation($"Received event: {eventGridEvent.EventType}");

		
		if (eventGridEvent.EventType == "OrderStatusChanged")
		{
			try
			{
				// Deserialize data directly into dynamic or DTO
				var eventData = JsonDocument.Parse(eventGridEvent.Data.ToString());

				var id = eventData.RootElement.GetProperty("id").GetString();

				string customerEmail = null;
				string customerName = null;

				// Check if customerEmail exists in the event data
				if (eventData.RootElement.TryGetProperty("CustomerEmail", out var emailProperty))
				{
					customerEmail = emailProperty.GetString();
				}

				// Check if customerName exists in the event data
				if (eventData.RootElement.TryGetProperty("CustomerName", out var nameProperty))
				{
					customerName = nameProperty.GetString();
				}

				var originalStatusValue = eventData.RootElement.GetProperty("OriginalStatus").GetInt32();
				var newStatusValue = eventData.RootElement.GetProperty("NewStatus").GetInt32();

				var originalStatus = (OrderStatus)originalStatusValue;
				var newStatus = (OrderStatus)newStatusValue;


				var lastUpdated = eventData.RootElement.GetProperty("LastUpdated").GetDateTime();

				if (originalStatus != newStatus)
				{
					_logger.LogInformation($"Order {id} status changed from {originalStatus} to {newStatus}");

					// Build the email body, using null checks for email and name
					string emailBody = BuildStatusEmailBody(customerName ?? "Customer", id, originalStatus, newStatus, lastUpdated);

					if (customerEmail != null)
					{
						await _emailService.SendEmailAsync(
							customerEmail,
							$"Order #{id} Status Update: {newStatus}",
							emailBody);
					}
					else
					{
						_logger.LogWarning($"No customer email provided for order {id}. Email not sent.");
					}
				}

			}
			catch (Exception ex)
			{
				_logger.LogError($"Error processing event: {ex}");
			}
		}
	}

	private string BuildStatusEmailBody(string customerName, string orderId, OrderStatus oldStatus, OrderStatus newStatus, DateTime timestamp)
	{
		return $@"
Dear {customerName},

Your order status has been updated:

Order ID: {orderId}
Previous Status: {oldStatus}
New Status: {newStatus}
Update Time: {timestamp:yyyy-MM-dd HH:mm:ss} UTC

Thank you for your business!
";
	}
}