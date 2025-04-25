using Azure;
using Azure.Storage.Blobs;
using FunctionAppAzure.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;

namespace FunctionAppAzure;
public static class ProcessOrderQueue
{

	[Function(nameof(ProcessOrderQueue))]
	public static async Task ProcessQueueMessage(
		[QueueTrigger("order-processing-queue", Connection = "ConnString")] string id,

		[CosmosDBInput(
			databaseName: "OrdersDB",
			containerName: "Orders",
			Connection = "CosmosDBConnection",
			Id = "{queueTrigger}",
			PartitionKey = "{queueTrigger}")] Order order,

		FunctionContext context)
	{
		var logger = context.GetLogger("ProcessOrder");
		logger.LogInformation($"Ruter ordre {order.Id} med status: {order.Status}");

		string invoice = GenerateSimpleTxtInvoice(order);
		await UploadInvoiceToBlob(order.Id, invoice, logger);

	}

	private static async Task UploadInvoiceToBlob(string orderId, string invoiceContent, ILogger logger)
	{
		var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("BlobConnection"));
		var containerClient = blobServiceClient.GetBlobContainerClient("invoices");

		try
		{
			await containerClient.CreateIfNotExistsAsync();
		}
		catch (RequestFailedException ex) when (ex.Status == 409) { /* Container exists */ }

		var blobClient = containerClient.GetBlobClient($"{orderId}.txt");
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invoiceContent));
		var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		await blobClient.UploadAsync(stream, overwrite: true, cts.Token);
	}

	private static string GenerateSimpleTxtInvoice(Order order)
	{
		var sb = new StringBuilder();

		sb.AppendLine("INVOICE");
		sb.AppendLine($"Order #: {order.Id}");
		sb.AppendLine($"Status: {order.Status}");
		sb.AppendLine($"Total: {order.Total:C}");
		sb.AppendLine();

		sb.AppendLine("Bill to:");
		sb.AppendLine($"Name: {order.CustomerName}");
		sb.AppendLine($"Email: {order.CustomerEmail}");
		sb.AppendLine();

		sb.AppendLine("Thank you for your business!");
		sb.AppendLine();

		sb.AppendLine("Payment Instructions:");
		sb.AppendLine("Please make payment to [YOUR BANK ACCOUNT]");


		return sb.ToString();
	}
}
