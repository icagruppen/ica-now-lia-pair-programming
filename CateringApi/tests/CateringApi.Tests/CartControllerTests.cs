using CateringApi.Controllers;
using CateringApi.DTOs;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace CateringApi.Tests;

public class CartControllerTests
{
    private readonly ICartService _mockCartService = Substitute.For<ICartService>();
    private readonly CartController _controller;

    public CartControllerTests()
    {
        _controller = new CartController(_mockCartService);
    }

    private static CartResponse MakeCart(string sessionId) =>
        new(1, sessionId, DateTime.UtcNow, DateTime.UtcNow, [], 0m);

    [Test]
    public async Task GetCart_WhenExists_ReturnsOk()
    {
        var cart = MakeCart("session-1");
        _mockCartService.GetCartAsync("session-1").Returns(cart);
        var result = await _controller.GetCart("session-1");
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(cart));
    }

    [Test]
    public async Task GetCart_WhenNotFound_ReturnsNotFound()
    {
        _mockCartService.GetCartAsync("missing").Returns((CartResponse?)null);
        var result = await _controller.GetCart("missing");
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task AddItem_ReturnsOk()
    {
        var request = new AddCartItemRequest(1, 2);
        var cart = MakeCart("session-1");
        _mockCartService.AddItemAsync("session-1", request).Returns(cart);
        var result = await _controller.AddItem("session-1", request);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(cart));
    }

    [Test]
    public async Task AddItem_WhenProductNotFound_ReturnsNotFound()
    {
        var request = new AddCartItemRequest(999, 1);
        _mockCartService.AddItemAsync("session-1", request).ThrowsAsync(new KeyNotFoundException());
        var result = await _controller.AddItem("session-1", request);
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateItem_WhenFound_ReturnsOk()
    {
        var request = new UpdateCartItemRequest(3);
        var cart = MakeCart("session-1");
        _mockCartService.UpdateItemAsync("session-1", 1, request).Returns(cart);
        var result = await _controller.UpdateItem("session-1", 1, request);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(cart));
    }

    [Test]
    public async Task RemoveItem_WhenFound_ReturnsOk()
    {
        var cart = MakeCart("session-1");
        _mockCartService.RemoveItemAsync("session-1", 1).Returns(cart);
        var result = await _controller.RemoveItem("session-1", 1);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(cart));
    }

    [Test]
    public async Task ClearCart_WhenExists_ReturnsNoContent()
    {
        _mockCartService.ClearCartAsync("session-1").Returns(true);
        var result = await _controller.ClearCart("session-1");
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task ClearCart_WhenNotFound_ReturnsNotFound()
    {
        _mockCartService.ClearCartAsync("missing").Returns(false);
        var result = await _controller.ClearCart("missing");
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
