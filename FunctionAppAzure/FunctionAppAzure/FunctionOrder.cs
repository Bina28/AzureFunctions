using FunctionAppAzure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FunctionAppAzure;


public static class FunctionOrder
{
	[Function("FunctionOrder")]

	public static async Task<OrderResponse> Run(
  [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
	FunctionContext context)
	{
		var logger = context.GetLogger("FunctionOrder");

		try
		{
			logger.LogInformation($"Processing new order.");
			var order = await req.ReadFromJsonAsync<Order>();
			if (string.IsNullOrEmpty(order.Id))
			{
				order.Id = Guid.NewGuid().ToString();
			}

			order.Status = OrderStatus.OrderPlaced;
			order.LastUpdated = DateTime.UtcNow;

			var response = req.CreateResponse(HttpStatusCode.OK);
			await response.WriteStringAsync($"Order {order.Id} received");

			return new OrderResponse
			{
				Document = order,
				HttpResponse = response,
				CosmosOutput = order,  // This will be saved to CosmosDB
				QueueMessage = order.Id  // This will go to the queue
			};
		}
		catch (Exception ex)
		{
			var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
			await errorResponse.WriteAsJsonAsync(new { Error = "Processing failed" });
			return new OrderResponse { HttpResponse = errorResponse };
		}
	}
}


