using System.ComponentModel.DataAnnotations;

namespace CateringApi.Models;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Preparing,
    Ready,
    Delivered,
    Cancelled
}

public class Order
{
    public int Id { get; set; }
    [Required] public string OrderNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string CustomerName { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string CustomerEmail { get; set; } = string.Empty;
    [MaxLength(50)] public string CustomerPhone { get; set; } = string.Empty;
    [MaxLength(500)] public string DeliveryAddress { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
}
