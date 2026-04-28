using CateringApi.DTOs;

namespace CateringApi.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponse>> GetAllAsync();
    Task<PaymentResponse?> GetByIdAsync(int id);
    Task<PaymentResponse?> GetByOrderIdAsync(int orderId);
    Task<PaymentResponse> InitiateAsync(InitiatePaymentRequest request);
    Task<PaymentResponse?> ProcessAsync(int id);
    Task<PaymentResponse?> RefundAsync(int id);
}
