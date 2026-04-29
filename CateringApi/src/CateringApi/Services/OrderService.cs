using CateringApi.Data;
using CateringApi.DTOs;
using CateringApi.Messaging.Events;
using CateringApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringApi.Services;

public class OrderService(CateringDbContext db, IServiceBusService serviceBus, ILogger<OrderService> logger) : IOrderService
{
    private static OrderResponse ToResponse(Order o) => new(
        o.Id, o.OrderNumber, o.CustomerId, o.CustomerName, o.CustomerEmail, o.CustomerPhone, o.DeliveryAddress,
        o.Status.ToString(), o.TotalAmount, o.CreatedAt, o.UpdatedAt,
        o.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice)));

    private IQueryable<Order> OrdersWithItems() => db.Orders.Include(o => o.Items);

    public async Task<IEnumerable<OrderResponse>> GetAllAsync(OrderStatus? status = null)
    {
        var query = OrdersWithItems();
        if (status.HasValue) query = query.Where(o => o.Status == status);
        return await query.OrderByDescending(o => o.CreatedAt).Select(o => new OrderResponse(o.Id, o.OrderNumber, o.CustomerId, o.CustomerName, o.CustomerEmail, o.CustomerPhone, o.DeliveryAddress, o.Status.ToString(), o.TotalAmount, o.CreatedAt, o.UpdatedAt, o.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice)))).ToListAsync();
    }

    public async Task<OrderResponse?> GetByIdAsync(int id)
    {
        var o = await OrdersWithItems().FirstOrDefaultAsync(o => o.Id == id);
        return o is null ? null : ToResponse(o);
    }

    public async Task<OrderResponse?> GetByOrderNumberAsync(string orderNumber)
    {
        var o = await OrdersWithItems().FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        return o is null ? null : ToResponse(o);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request)
    {
        var cartItems = await db.CartItems.Include(ci => ci.Product)
            .Where(ci => ci.Cart.SessionId == request.SessionId).ToListAsync();

        var orderItems = cartItems.Select(ci => new OrderItem
        {
            ProductId = ci.ProductId,
            ProductName = ci.Product.Name,
            Quantity = ci.Quantity,
            UnitPrice = ci.UnitPrice
        }).ToList();

        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            DeliveryAddress = request.DeliveryAddress,
            Items = orderItems,
            TotalAmount = orderItems.Sum(i => i.Quantity * i.UnitPrice)
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Clear cart
        var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.SessionId == request.SessionId);
        if (cart is not null) { db.CartItems.RemoveRange(cart.Items); db.Carts.Remove(cart); await db.SaveChangesAsync(); }

        logger.LogInformation("Created order {OrderId}: {OrderNumber}", order.Id, order.OrderNumber);
        await serviceBus.SendAsync(new OrderCreatedEvent(order.Id, order.OrderNumber, order.CustomerEmail, order.TotalAmount, order.CreatedAt));
        return ToResponse(order);
    }

    public async Task<OrderResponse?> UpdateAsync(int id, UpdateOrderRequest request)
    {
        var o = await OrdersWithItems().FirstOrDefaultAsync(o => o.Id == id);
        if (o is null) return null;
        o.CustomerName = request.CustomerName; o.CustomerEmail = request.CustomerEmail;
        o.CustomerPhone = request.CustomerPhone; o.DeliveryAddress = request.DeliveryAddress;
        o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated order {OrderId}", id);
        return ToResponse(o);
    }

    public async Task<OrderResponse?> UpdateStatusAsync(int id, OrderStatus status)
    {
        var o = await OrdersWithItems().FirstOrDefaultAsync(o => o.Id == id);
        if (o is null) return null;
        var oldStatus = o.Status;
        o.Status = status; o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Order {OrderId} status changed from {OldStatus} to {NewStatus}", id, oldStatus, status);
        await serviceBus.SendAsync(new OrderStatusChangedEvent(o.Id, o.OrderNumber, oldStatus, status, o.UpdatedAt));
        return ToResponse(o);
    }

    public async Task<bool> CancelAsync(int id)
    {
        var o = await db.Orders.FindAsync(id);
        if (o is null) return false;
        o.Status = OrderStatus.Cancelled; o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Cancelled order {OrderId}", id);
        return true;
    }
}
