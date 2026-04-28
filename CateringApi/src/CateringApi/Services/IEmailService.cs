namespace CateringApi.Services;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
    Task SendOrderConfirmationAsync(int orderId);
    Task SendPaymentReceiptAsync(int paymentId);
}
