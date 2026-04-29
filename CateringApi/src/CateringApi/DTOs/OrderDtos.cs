using System.ComponentModel.DataAnnotations;
using CateringApi.Models;

namespace CateringApi.DTOs;

public record OrderResponse(int Id, string OrderNumber, string CustomerId, string CustomerName, string CustomerEmail, string CustomerPhone, string DeliveryAddress, string Status, decimal TotalAmount, DateTime CreatedAt, DateTime UpdatedAt, IEnumerable<OrderItemResponse> Items);

public record OrderItemResponse(int Id, int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public record CreateOrderRequest(
    string SessionId,
    string CustomerId,
    [Required, MaxLength(200)] string CustomerName,
    [Required, EmailAddress, MaxLength(200)] string CustomerEmail,
    [MaxLength(50)] string CustomerPhone,
    [MaxLength(500)] string DeliveryAddress);

public record UpdateOrderRequest(
    [Required, MaxLength(200)] string CustomerName,
    [Required, EmailAddress, MaxLength(200)] string CustomerEmail,
    [MaxLength(50)] string CustomerPhone,
    [MaxLength(500)] string DeliveryAddress);

public record UpdateOrderStatusRequest([Required] OrderStatus Status);
