namespace FunctionAppAzure.Models;

public enum OrderStatus
{
	OrderPlaced = 1,
	PaymentComplete = 2,
	OrderShipped = 3,
	ShipmentFulfilled = 4
}