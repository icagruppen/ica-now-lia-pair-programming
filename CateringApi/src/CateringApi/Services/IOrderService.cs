using CateringApi.DTOs;
using CateringApi.Models;

namespace CateringApi.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderResponse>> GetAllAsync(OrderStatus? status = null);
    Task<OrderResponse?> GetByIdAsync(int id);
    Task<OrderResponse?> GetByOrderNumberAsync(string orderNumber);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request);
    Task<OrderResponse?> UpdateAsync(int id, UpdateOrderRequest request);
    Task<OrderResponse?> UpdateStatusAsync(int id, OrderStatus status);
    Task<bool> CancelAsync(int id);
}
