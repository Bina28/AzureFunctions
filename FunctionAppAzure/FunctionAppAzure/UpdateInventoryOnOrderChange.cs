using FunctionAppAzure.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FunctionAppAzure;


public class UpdateInventoryOnOrderChange
{
	private readonly ILogger<UpdateInventoryOnOrderChange> _logger;
	private readonly CosmosClient _cosmosClient;

	// Inject CosmosClient through DI
	public UpdateInventoryOnOrderChange(
		ILogger<UpdateInventoryOnOrderChange> logger,
		CosmosClient cosmosClient)
	{
		_logger = logger;
		_cosmosClient = cosmosClient;
	}

	[Function(nameof(UpdateInventoryOnOrderChange))]
	public async Task Run(
		[CosmosDBTrigger(
		databaseName: "OrdersDB",
		containerName: "Orders",
		Connection = "CosmosDBConnection",
		LeaseContainerName = "leases")] IReadOnlyList<Order> orders)
	{
		if (orders == null || !orders.Any()) return;

		var inventoryContainer = _cosmosClient.GetContainer("OrdersDB", "InventoryItem");

		foreach (var order in orders)
		{
			// Only process paid orders (status 2 = PaymentComplete)
			if (order.Status != OrderStatus.PaymentComplete)
			{
				_logger.LogInformation($"Skipping order {order.Id} with status {order.Status}");
				continue;
			}

			_logger.LogInformation($"Processing order {order.Id} for inventory update");

			foreach (var orderItem in order.Items)
			{
				try
				{
					// 1. Read the FULL existing document
					var response = await inventoryContainer.ReadItemAsync<InventoryItem>(
						id: orderItem.ProductId,
						partitionKey: new PartitionKey(orderItem.ProductId));

					var inventoryItem = response.Resource;
					_logger.LogInformation($"Current stock for {inventoryItem.Id}: {inventoryItem.Quantity}");

					// 2. Update only the necessary fields
					inventoryItem.Quantity -= orderItem.Quantity;
				

					// 3. Replace the ENTIRE document while maintaining all fields
					await inventoryContainer.ReplaceItemAsync(
						item: inventoryItem,
						id: inventoryItem.Id,
						partitionKey: new PartitionKey(inventoryItem.Id));

					_logger.LogInformation($"Successfully updated {inventoryItem.Id}. New quantity: {inventoryItem.Quantity}");
				}
				catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
				{
					_logger.LogError($"Inventory item {orderItem.ProductId} not found");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to update inventory for {orderItem.ProductId}");
				
				}
			}
		}
		}
	}

