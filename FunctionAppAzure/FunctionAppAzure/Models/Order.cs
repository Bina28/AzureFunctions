using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FunctionAppAzure.Models;


public class Order
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[Required]
	public string CustomerName { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	public string CustomerEmail { get; set; } = string.Empty;

	[Required]
	public OrderStatus Status { get; set; }

	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

	[MinLength(1)]

	public List<OrderItem> Items { get; set; } = new List<OrderItem>();

	[Range(0, double.MaxValue)]
	public decimal Total { get; set; }

}

