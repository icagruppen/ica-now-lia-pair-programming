using CateringApi.Models;

namespace CateringApi.Messaging.Events;

public record OrderStatusChangedEvent(int OrderId, string OrderNumber, OrderStatus OldStatus, OrderStatus NewStatus, DateTime UpdatedAt);
