using FunctionAppAzure.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;


namespace FunctionAppAzure;

public class OrderResponse
{
	public Order Document { get; set; }
	public HttpResponseData HttpResponse { get; set; }

	[CosmosDBOutput(
		databaseName: "OrdersDB",
		containerName: "Orders",
		Connection = "CosmosDBConnection")]
	public Order CosmosOutput { get; set; }

	[QueueOutput("order-processing-queue", Connection = "ConnString")]
	public string QueueMessage { get; set; }


}