using Azure.Core;
using FunctionAppAzure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FunctionAppAzure;



public static class SimulateOrderFlow
{
	[Function("SimulateOrderFlow")]
	public static async Task<HttpResponseData> Run(
		[HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
		[CosmosDBInput(
			databaseName: "OrdersDB",
			containerName: "Orders",
			Connection = "CosmosDBConnection")] CosmosClient cosmosClient,
		FunctionContext context)
	{
		

		var logger = context.GetLogger("SimulateOrderFlow");

		// Step 1: Read body
		var requestData = await req.ReadFromJsonAsync<OrderUpdateRequest>();

		if (string.IsNullOrEmpty(requestData?.Id) || requestData.Status == 0)
		{
			var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
			await badRequest.WriteStringAsync("Missing 'id' or 'status'");
			return badRequest;
		}

		// Step 2: Parse status as int
		if (!Enum.IsDefined(typeof(OrderStatus), requestData.Status))
		{
			var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
			await badRequest.WriteStringAsync("Invalid status value.");
			return badRequest;
		}

		var newStatus = (OrderStatus)requestData.Status;


		var container = cosmosClient.GetContainer("OrdersDB", "Orders");

		try
		{
			// Step 3: Read order from CosmosDB
			var orderResponse = await container.ReadItemAsync<Order>(requestData.Id, new PartitionKey(requestData.Id));
			var order = orderResponse.Resource;

			var originalStatus = order.Status;

			if (newStatus != order.Status)
			{
				order.Status = newStatus;
				order.LastUpdated = DateTime.UtcNow;

				await container.ReplaceItemAsync(order, order.Id, new PartitionKey(order.Id));
				logger.LogInformation("Order updated in Cosmos DB.");

				await PublishStatusChangeEvent.PublishStatusChange(order, originalStatus, logger);
			}
			else
			{
				logger.LogInformation("No status change. Skipping event publish.");
			}

			var ok = req.CreateResponse(HttpStatusCode.OK);
			await ok.WriteStringAsync($"Order {order.Id} updated to status {order.Status}");
			return ok;
		}
		catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
		{
			var notFound = req.CreateResponse(HttpStatusCode.NotFound);
			await notFound.WriteStringAsync($"Order with id '{requestData.Id}' not found.");
			return notFound;
		}
	}

	public class OrderUpdateRequest
	{
		public string Id { get; set; }
		public int Status { get; set; }
	}
}

