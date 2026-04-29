using CateringApi.DTOs;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CateringApi.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController(ICartService cartService) : ControllerBase
{
    [HttpGet("{sessionId}")] public async Task<IActionResult> GetCart(string sessionId)
    {
        var cart = await cartService.GetCartAsync(sessionId);
        return cart is null ? NotFound() : Ok(cart);
    }
    [HttpPost("{sessionId}/items")] public async Task<IActionResult> AddItem(string sessionId, [FromBody] AddCartItemRequest request)
    {
        try { return Ok(await cartService.AddItemAsync(sessionId, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
    [HttpPut("{sessionId}/items/{itemId}")] public async Task<IActionResult> UpdateItem(string sessionId, int itemId, [FromBody] UpdateCartItemRequest request)
    {
        var result = await cartService.UpdateItemAsync(sessionId, itemId, request);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpDelete("{sessionId}/items/{itemId}")] public async Task<IActionResult> RemoveItem(string sessionId, int itemId)
    {
        var result = await cartService.RemoveItemAsync(sessionId, itemId);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpDelete("{sessionId}")] public async Task<IActionResult> ClearCart(string sessionId)
    {
        var cleared = await cartService.ClearCartAsync(sessionId);
        return cleared ? NoContent() : NotFound();
    }
}
