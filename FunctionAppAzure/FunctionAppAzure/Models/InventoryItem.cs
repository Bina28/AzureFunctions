using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace FunctionAppAzure.Models;

public class InventoryItem
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("quantity")]
	public int Quantity { get; set; }

	[JsonProperty("price")]
	public decimal Price { get; set; }


}
