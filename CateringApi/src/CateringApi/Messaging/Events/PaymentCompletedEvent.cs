namespace CateringApi.Messaging.Events;

public record PaymentCompletedEvent(int PaymentId, int OrderId, decimal Amount, string TransactionId, DateTime CompletedAt);
