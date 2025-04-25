using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FunctionAppAzure.Models;

public class OrderItem
{
	[JsonPropertyName("id")]
	[Required(ErrorMessage = "ProductId is required.")]
	public string ProductId { get; set; } = string.Empty;

	[JsonPropertyName("quantity")]
	[Required(ErrorMessage = "Quantity is required.")]
	[Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
	public int Quantity { get; set; }
	public string Name { get; set; }
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
