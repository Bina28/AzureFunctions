using System;
using System.Configuration;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionAppAzure
{
	public static class ProcessOrderQueue
	{
		[Function(nameof(ProcessOrderQueue))]
		public static async Task<Order> ProcessQueueMessage(
			  [QueueTrigger("order-processing-queue", Connection = "AzureWebJobsStorage")] string orderId,
	[CosmosDBInput(
		databaseName: "OrdersDB",
		containerName: "Orders",
		Connection = "CosmosDBConnection",
		Id = "{queueTrigger}")] Order order,
	FunctionContext context)
		{
			var logger = context.GetLogger("ProcessOrder");

			order.Status = order.Status switch
			{
				OrderStatus.OrderPlaced => OrderStatus.PaymentComplete,
				OrderStatus.PaymentComplete => OrderStatus.OrderShipped,
				OrderStatus.OrderShipped=> OrderStatus.ShipmentFulfilled,
				_ => order.Status
			};

			order.LastUpdated = DateTime.UtcNow;

			return order;  // The return value wi

		}
	}
}