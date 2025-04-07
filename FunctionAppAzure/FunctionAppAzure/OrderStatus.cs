using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionAppAzure;

public enum OrderStatus
{
	OrderPlaced = 1,
	PaymentComplete = 2,
	OrderShipped = 3,
	ShipmentFulfilled = 4
}