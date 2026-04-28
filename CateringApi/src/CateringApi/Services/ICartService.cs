using CateringApi.DTOs;

namespace CateringApi.Services;

public interface ICartService
{
    Task<CartResponse?> GetCartAsync(string sessionId);
    Task<CartResponse> AddItemAsync(string sessionId, AddCartItemRequest request);
    Task<CartResponse?> UpdateItemAsync(string sessionId, int itemId, UpdateCartItemRequest request);
    Task<CartResponse?> RemoveItemAsync(string sessionId, int itemId);
    Task<bool> ClearCartAsync(string sessionId);
}
