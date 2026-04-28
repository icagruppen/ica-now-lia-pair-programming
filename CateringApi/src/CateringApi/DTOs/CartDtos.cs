using System.ComponentModel.DataAnnotations;

namespace CateringApi.DTOs;

public record CartResponse(int Id, string SessionId, DateTime CreatedAt, DateTime UpdatedAt, IEnumerable<CartItemResponse> Items, decimal TotalAmount);

public record CartItemResponse(int Id, int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public record AddCartItemRequest(
    [Range(1, int.MaxValue)] int ProductId,
    [Range(1, int.MaxValue)] int Quantity);

public record UpdateCartItemRequest([Range(1, int.MaxValue)] int Quantity);
