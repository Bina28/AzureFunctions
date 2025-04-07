using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace FunctionAppAzure;


public static class FunctionOrder
{
	[Function("FunctionOrder")]

	public static async Task<OrderResponse> Run(
  [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
	FunctionContext context)
	{
		var logger = context.GetLogger("ReceiveOrder");

		var order = await req.ReadFromJsonAsync<Order>();
		order.id = Guid.NewGuid().ToString();
		order.Status = OrderStatus.OrderPlaced;
		order.LastUpdated = DateTime.UtcNow;

		var response = req.CreateResponse(HttpStatusCode.OK);
		await response.WriteStringAsync($"Order {order.id} received");

		return new OrderResponse
		{
			Document = order,
			HttpResponse = response,
			CosmosOutput = order,  // This will be saved to CosmosDB
			QueueMessage = order.id  // This will go to the queue
		};
	}
}


