using System.ComponentModel.DataAnnotations;

namespace CateringApi.DTOs;

public record PaymentResponse(int Id, int OrderId, decimal Amount, string Status, string PaymentMethod, string TransactionId, DateTime CreatedAt, DateTime UpdatedAt);

public record InitiatePaymentRequest(
    [Range(1, int.MaxValue)] int OrderId,
    [Required] string PaymentMethod);

public record EmailRequest(
    [Required, EmailAddress] string To,
    [Required] string Subject,
    [Required] string Body);
