using Newtonsoft.Json;

namespace FunctionAppAzure;


public class Order
{
	[JsonProperty("id")]
	public string id { get; set; }

	public string customerName { get; set; }
	
	public OrderStatus Status { get; set; } = OrderStatus.OrderPlaced;
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

}
