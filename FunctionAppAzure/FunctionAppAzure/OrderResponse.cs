using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System.Configuration;


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

	[QueueOutput("order-processing-queue", Connection = "AzureWebJobsStorage")]
	public string QueueMessage { get; set; }
}