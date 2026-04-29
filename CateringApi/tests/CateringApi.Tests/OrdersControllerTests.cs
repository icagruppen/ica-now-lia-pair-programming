using CateringApi.Controllers;
using CateringApi.DTOs;
using CateringApi.Models;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace CateringApi.Tests;

public class OrdersControllerTests
{
    private readonly IOrderService _mockOrderService = Substitute.For<IOrderService>();
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _controller = new OrdersController(_mockOrderService);
    }

    [Test]
    public async Task GetAll_ReturnsOkWithOrders()
    {
        var orders = new List<OrderResponse>
        {
            new(1, "ORD-001", "c1", "Alice", "alice@test.com", "555-0001", "1 Main St", "Pending", 100m, DateTime.UtcNow, DateTime.UtcNow, [])
        };
        _mockOrderService.GetAllAsync(null).Returns(orders);
        var result = await _controller.GetAll(null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(orders));
    }

    [Test]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var order = new OrderResponse(1, "ORD-001", "c1", "Alice", "alice@test.com", "555-0001", "1 Main St", "Pending", 100m, DateTime.UtcNow, DateTime.UtcNow, []);
        _mockOrderService.GetByIdAsync(1).Returns(order);
        var result = await _controller.GetById(1);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(order));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _mockOrderService.GetByIdAsync(99).Returns((OrderResponse?)null);
        var result = await _controller.GetById(99);
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_ReturnsCreated()
    {
        var request = new CreateOrderRequest("sess-1", "c1", "Alice", "alice@test.com", "555-0001", "1 Main St");
        var order = new OrderResponse(1, "ORD-001", "c1", "Alice", "alice@test.com", "555-0001", "1 Main St", "Pending", 100m, DateTime.UtcNow, DateTime.UtcNow, []);
        _mockOrderService.CreateAsync(request).Returns(order);
        var result = await _controller.Create(request);
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        var created = (CreatedAtActionResult)result;
        Assert.That(created.Value, Is.EqualTo(order));
    }

    [Test]
    public async Task UpdateStatus_WhenFound_ReturnsOk()
    {
        var req = new UpdateOrderStatusRequest(OrderStatus.Confirmed);
        var order = new OrderResponse(1, "ORD-001", "c1", "Alice", "alice@test.com", "555-0001", "1 Main St", "Confirmed", 100m, DateTime.UtcNow, DateTime.UtcNow, []);
        _mockOrderService.UpdateStatusAsync(1, OrderStatus.Confirmed).Returns(order);
        var result = await _controller.UpdateStatus(1, req);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(order));
    }

    [Test]
    public async Task Cancel_WhenFound_ReturnsNoContent()
    {
        _mockOrderService.CancelAsync(1).Returns(true);
        var result = await _controller.Cancel(1);
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Cancel_WhenNotFound_ReturnsNotFound()
    {
        _mockOrderService.CancelAsync(99).Returns(false);
        var result = await _controller.Cancel(99);
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
