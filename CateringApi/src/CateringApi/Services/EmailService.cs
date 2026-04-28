using CateringApi.Data;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace CateringApi.Services;

public class EmailService(CateringDbContext db, IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    private bool IsConfigured()
    {
        var host = configuration["Email:SmtpHost"];
        return !string.IsNullOrWhiteSpace(host);
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        if (!IsConfigured()) { logger.LogWarning("SMTP not configured, skipping email to {To}", to); return; }
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(configuration["Email:FromName"], configuration["Email:FromAddress"] ?? string.Empty));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };
            using var client = new SmtpClient();
            await client.ConnectAsync(configuration["Email:SmtpHost"] ?? string.Empty, int.Parse(configuration["Email:SmtpPort"] ?? "587"));
            await client.AuthenticateAsync(configuration["Email:Username"] ?? string.Empty, configuration["Email:Password"] ?? string.Empty);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            logger.LogInformation("Sent email to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }

    public async Task SendOrderConfirmationAsync(int orderId)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) { logger.LogWarning("Order {OrderId} not found for confirmation email", orderId); return; }
        var body = $"<h1>Order Confirmed</h1><p>Dear {order.CustomerName},</p><p>Your order <strong>{order.OrderNumber}</strong> has been confirmed. Total: {order.TotalAmount:C}</p>";
        await SendAsync(order.CustomerEmail, $"Order Confirmation - {order.OrderNumber}", body);
    }

    public async Task SendPaymentReceiptAsync(int paymentId)
    {
        var payment = await db.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment is null) { logger.LogWarning("Payment {PaymentId} not found for receipt email", paymentId); return; }
        var body = $"<h1>Payment Receipt</h1><p>Dear {payment.Order.CustomerName},</p><p>Payment of {payment.Amount:C} for order {payment.Order.OrderNumber} has been processed. Transaction ID: {payment.TransactionId}</p>";
        await SendAsync(payment.Order.CustomerEmail, $"Payment Receipt - {payment.Order.OrderNumber}", body);
    }
}
