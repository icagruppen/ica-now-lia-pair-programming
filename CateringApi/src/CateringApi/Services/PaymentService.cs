using CateringApi.Data;
using CateringApi.DTOs;
using CateringApi.Messaging.Events;
using CateringApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringApi.Services;

public class PaymentService(CateringDbContext db, IServiceBusService serviceBus, ILogger<PaymentService> logger) : IPaymentService
{
    private static PaymentResponse ToResponse(Payment p) =>
        new(p.Id, p.OrderId, p.Amount, p.Status.ToString(), p.PaymentMethod, p.TransactionId, p.CreatedAt, p.UpdatedAt);

    public async Task<IEnumerable<PaymentResponse>> GetAllAsync() =>
        await db.Payments.Select(p => new PaymentResponse(p.Id, p.OrderId, p.Amount, p.Status.ToString(), p.PaymentMethod, p.TransactionId, p.CreatedAt, p.UpdatedAt)).ToListAsync();

    public async Task<PaymentResponse?> GetByIdAsync(int id)
    {
        var p = await db.Payments.FindAsync(id);
        return p is null ? null : ToResponse(p);
    }

    public async Task<PaymentResponse?> GetByOrderIdAsync(int orderId)
    {
        var p = await db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        return p is null ? null : ToResponse(p);
    }

    public async Task<PaymentResponse> InitiateAsync(InitiatePaymentRequest request)
    {
        var order = await db.Orders.FindAsync(request.OrderId) ?? throw new KeyNotFoundException($"Order {request.OrderId} not found");
        var payment = new Payment { OrderId = request.OrderId, Amount = order.TotalAmount, PaymentMethod = request.PaymentMethod, Status = PaymentStatus.Pending };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        logger.LogInformation("Initiated payment {PaymentId} for order {OrderId}", payment.Id, request.OrderId);
        return ToResponse(payment);
    }

    public async Task<PaymentResponse?> ProcessAsync(int id)
    {
        var p = await db.Payments.FindAsync(id);
        if (p is null) return null;
        p.Status = PaymentStatus.Completed;
        p.TransactionId = $"TXN-{Guid.NewGuid():N}";
        p.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Processed payment {PaymentId}, transaction {TransactionId}", id, p.TransactionId);
        await serviceBus.SendAsync(new PaymentCompletedEvent(p.Id, p.OrderId, p.Amount, p.TransactionId, p.UpdatedAt));
        return ToResponse(p);
    }

    public async Task<PaymentResponse?> RefundAsync(int id)
    {
        var p = await db.Payments.FindAsync(id);
        if (p is null) return null;
        p.Status = PaymentStatus.Refunded;
        p.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Refunded payment {PaymentId}", id);
        return ToResponse(p);
    }
}
