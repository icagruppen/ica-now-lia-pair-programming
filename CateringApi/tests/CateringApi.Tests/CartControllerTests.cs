using CateringApi.Controllers;
using CateringApi.DTOs;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CateringApi.Tests;

public class CartControllerTests
{
    private readonly Mock<ICartService> _mockCartService = new();
    private readonly CartController _controller;

    public CartControllerTests()
    {
        _controller = new CartController(_mockCartService.Object);
    }

    private static CartResponse MakeCart(string sessionId) =>
        new(1, sessionId, DateTime.UtcNow, DateTime.UtcNow, [], 0m);

    [Fact]
    public async Task GetCart_WhenExists_ReturnsOk()
    {
        var cart = MakeCart("session-1");
        _mockCartService.Setup(s => s.GetCartAsync("session-1")).ReturnsAsync(cart);
        var result = await _controller.GetCart("session-1");
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(cart, ok.Value);
    }

    [Fact]
    public async Task GetCart_WhenNotFound_ReturnsNotFound()
    {
        _mockCartService.Setup(s => s.GetCartAsync("missing")).ReturnsAsync((CartResponse?)null);
        var result = await _controller.GetCart("missing");
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddItem_ReturnsOk()
    {
        var request = new AddCartItemRequest(1, 2);
        var cart = MakeCart("session-1");
        _mockCartService.Setup(s => s.AddItemAsync("session-1", request)).ReturnsAsync(cart);
        var result = await _controller.AddItem("session-1", request);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(cart, ok.Value);
    }

    [Fact]
    public async Task AddItem_WhenProductNotFound_ReturnsNotFound()
    {
        var request = new AddCartItemRequest(999, 1);
        _mockCartService.Setup(s => s.AddItemAsync("session-1", request)).ThrowsAsync(new KeyNotFoundException());
        var result = await _controller.AddItem("session-1", request);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateItem_WhenFound_ReturnsOk()
    {
        var request = new UpdateCartItemRequest(3);
        var cart = MakeCart("session-1");
        _mockCartService.Setup(s => s.UpdateItemAsync("session-1", 1, request)).ReturnsAsync(cart);
        var result = await _controller.UpdateItem("session-1", 1, request);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(cart, ok.Value);
    }

    [Fact]
    public async Task RemoveItem_WhenFound_ReturnsOk()
    {
        var cart = MakeCart("session-1");
        _mockCartService.Setup(s => s.RemoveItemAsync("session-1", 1)).ReturnsAsync(cart);
        var result = await _controller.RemoveItem("session-1", 1);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(cart, ok.Value);
    }

    [Fact]
    public async Task ClearCart_WhenExists_ReturnsNoContent()
    {
        _mockCartService.Setup(s => s.ClearCartAsync("session-1")).ReturnsAsync(true);
        var result = await _controller.ClearCart("session-1");
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ClearCart_WhenNotFound_ReturnsNotFound()
    {
        _mockCartService.Setup(s => s.ClearCartAsync("missing")).ReturnsAsync(false);
        var result = await _controller.ClearCart("missing");
        Assert.IsType<NotFoundResult>(result);
    }
}
