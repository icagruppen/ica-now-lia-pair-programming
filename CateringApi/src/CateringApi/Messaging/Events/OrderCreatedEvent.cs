namespace CateringApi.Messaging.Events;

public record OrderCreatedEvent(int OrderId, string OrderNumber, string CustomerEmail, decimal TotalAmount, DateTime CreatedAt);
