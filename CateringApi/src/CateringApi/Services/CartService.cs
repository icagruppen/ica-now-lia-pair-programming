using CateringApi.Data;
using CateringApi.DTOs;
using CateringApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringApi.Services;

public class CartService(CateringDbContext db, ILogger<CartService> logger) : ICartService
{
    private static CartResponse ToResponse(Cart cart) => new(
        cart.Id, cart.SessionId, cart.CreatedAt, cart.UpdatedAt,
        cart.Items.Select(i => new CartItemResponse(i.Id, i.ProductId, i.Product?.Name ?? "", i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice)),
        cart.Items.Sum(i => i.Quantity * i.UnitPrice));

    private async Task<Cart?> GetCartWithItemsAsync(string sessionId) =>
        await db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.SessionId == sessionId);

    public async Task<CartResponse?> GetCartAsync(string sessionId)
    {
        var cart = await GetCartWithItemsAsync(sessionId);
        return cart is null ? null : ToResponse(cart);
    }

    public async Task<CartResponse> AddItemAsync(string sessionId, AddCartItemRequest request)
    {
        var cart = await GetCartWithItemsAsync(sessionId);
        if (cart is null)
        {
            cart = new Cart { SessionId = sessionId };
            db.Carts.Add(cart);
        }

        var product = await db.Products.FindAsync(request.ProductId) ?? throw new KeyNotFoundException($"Product {request.ProductId} not found");
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existing is not null)
            existing.Quantity += request.Quantity;
        else
            cart.Items.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity, UnitPrice = product.Price });

        cart.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await db.Entry(cart).Collection(c => c.Items).Query().Include(i => i.Product).LoadAsync();
        logger.LogInformation("Added product {ProductId} to cart {SessionId}", request.ProductId, sessionId);
        return ToResponse(cart);
    }

    public async Task<CartResponse?> UpdateItemAsync(string sessionId, int itemId, UpdateCartItemRequest request)
    {
        var cart = await GetCartWithItemsAsync(sessionId);
        if (cart is null) return null;
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return null;
        item.Quantity = request.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated cart item {ItemId} quantity to {Quantity}", itemId, request.Quantity);
        return ToResponse(cart);
    }

    public async Task<CartResponse?> RemoveItemAsync(string sessionId, int itemId)
    {
        var cart = await GetCartWithItemsAsync(sessionId);
        if (cart is null) return null;
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return null;
        cart.Items.Remove(item);
        db.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Removed cart item {ItemId}", itemId);
        return ToResponse(cart);
    }

    public async Task<bool> ClearCartAsync(string sessionId)
    {
        var cart = await GetCartWithItemsAsync(sessionId);
        if (cart is null) return false;
        db.CartItems.RemoveRange(cart.Items);
        db.Carts.Remove(cart);
        await db.SaveChangesAsync();
        logger.LogInformation("Cleared cart {SessionId}", sessionId);
        return true;
    }
}
