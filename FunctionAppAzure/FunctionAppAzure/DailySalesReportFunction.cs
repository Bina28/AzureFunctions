using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionAppAzure
{
    public class DailySalesReportFunction
    {


		[Function("DailySalesReportFunction")]
		public async Task RunAsync(
		[TimerTrigger("0 */5 * * * *")] TimerInfo myTimer,
		[CosmosDBInput(
		databaseName: "OrdersDB",
		containerName: "Orders",
		Connection = "CosmosDBConnection")] CosmosClient cosmosClient,
		FunctionContext context) // Add FunctionContext parameter
		{
			// Get logger from the context
			var logger = context.GetLogger("DailySalesReportFunction");

			logger.LogInformation($"Generating customer names report at: {DateTime.UtcNow}");

			try
			{
				// 1. Verify connection
				var database = cosmosClient.GetDatabase("OrdersDB");
				await database.ReadAsync();

				// 2. Define date range
				DateTime startDate = DateTime.UtcNow.AddHours(-24);
				DateTime endDate = DateTime.UtcNow;

				// 3. Build parameterized query
				var queryDefinition = new QueryDefinition(
					"SELECT VALUE c.customerName FROM c " +
					"WHERE c.LastUpdated >= @startDate " +
					"AND c.LastUpdated <= @endDate")
					.WithParameter("@startDate", startDate)
					.WithParameter("@endDate", endDate);

				// 4. Execute query
				var container = cosmosClient.GetContainer("OrdersDB", "Orders");
				var customerNames = new List<string>();

				using (var resultSet = container.GetItemQueryIterator<string>(queryDefinition))
				{
					while (resultSet.HasMoreResults)
					{
						var response = await resultSet.ReadNextAsync();
						customerNames.AddRange(response);
					}
				}

				logger.LogInformation($"Found {customerNames.Count} customers");

				// 5. Save report
				var reportContainer = cosmosClient.GetContainer("OrdersDB", "SalesReport");
				var report = new
				{
					id = Guid.NewGuid().ToString(),
					ReportDate = DateTime.UtcNow,
					CustomerCount = customerNames.Count,
					CustomerNames = customerNames
				};

				await reportContainer.UpsertItemAsync(report);
				logger.LogInformation("Report saved successfully");
			}
			catch (CosmosException ce)
			{
				logger.LogError($"Cosmos DB Error: {ce.StatusCode} - {ce.Message}");
				throw;
			}
			catch (Exception ex)
			{
				logger.LogError($"General Error: {ex.Message}");
				throw;
			}
		}

	}
}
