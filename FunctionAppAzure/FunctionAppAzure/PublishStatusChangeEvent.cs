using Azure;
using Azure.Messaging.EventGrid;
using FunctionAppAzure.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FunctionAppAzure;

public static class PublishStatusChangeEvent
{
	public static async Task PublishStatusChange(Order order, OrderStatus originalStatus, ILogger logger)
	{
		try
		{
			var eventGridEndpoint = Environment.GetEnvironmentVariable("EventGridEndpoint");
			var eventGridKey = Environment.GetEnvironmentVariable("EventGridKey");

			if (string.IsNullOrEmpty(eventGridEndpoint) || string.IsNullOrEmpty(eventGridKey))
			{
				logger.LogWarning("EventGrid configuration missing.");
				return;
			}

			var client = new EventGridPublisherClient(
				new Uri(eventGridEndpoint),
				new AzureKeyCredential(eventGridKey));

			var eventData = new
			{
				id = order.Id,
				NewStatus = (int)order.Status,  // Cast to int
				OriginalStatus = (int)originalStatus,  // Cast to int
				LastUpdated = DateTime.UtcNow,
				CustomerEmail = order.CustomerEmail,
				CustomerName = order.CustomerName // Legg til CustomerName og CustomerEmail for å bruke dem i e-posten
			};


			var eventToSend = new EventGridEvent(
				subject: $"order/{order.Id}",
				eventType: "OrderStatusChanged",
				dataVersion: "1.0",
				data: eventData);

			await client.SendEventAsync(eventToSend);
			logger.LogInformation($"Event sent for order {order.Id}");
			logger.LogInformation($"Sending event to: {eventGridEndpoint}");
			logger.LogInformation($"Event data: {JsonSerializer.Serialize(eventData)}");
		}
		catch (Exception ex)
		{
			logger.LogError($"Failed to publish event: {ex.Message}");
		}
	}
}