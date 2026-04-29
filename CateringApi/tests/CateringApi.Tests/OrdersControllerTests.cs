using CateringApi.Controllers;
using CateringApi.DTOs;
using CateringApi.Models;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CateringApi.Tests;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService = new();
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _controller = new OrdersController(_mockOrderService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithOrders()
    {
        var orders = new List<OrderResponse>
        {
            new(1, "ORD-001", "c1", "Alice", "alice@test.com", "555-0001", "1 Main St", "Pending", 100m, DateTime.UtcNow, DateTime.UtcNow, [])
        };
        _mockOrderService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(orders);
        var result = await _controller.GetAll(null);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(orders, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var order = new OrderResponse(1, "ORD-001", "c1", "Alice", "alice@test.com", "555-0001", "1 Main St", "Pending", 100m, DateTime.UtcNow, DateTime.UtcNow, []);
        _mockOrderService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(order);
        var result = await _controller.GetById(1);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(order, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _mockOrderService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((OrderResponse?)null);
        var result = await _controller.GetById(99);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var request = new CreateOrderRequest("sess-1", "c1", "Alice", "alice@test.com", "555-0001", "1 Main St");
        var order = new OrderResponse(1, "ORD-001", "c1", "Alice", "alice@test.com", "555-0001", "1 Main St", "Pending", 100m, DateTime.UtcNow, DateTime.UtcNow, []);
        _mockOrderService.Setup(s => s.CreateAsync(request)).ReturnsAsync(order);
        var result = await _controller.Create(request);
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(order, created.Value);
    }

    [Fact]
    public async Task UpdateStatus_WhenFound_ReturnsOk()
    {
        var req = new UpdateOrderStatusRequest(OrderStatus.Confirmed);
        var order = new OrderResponse(1, "ORD-001", "c1", "Alice", "alice@test.com", "555-0001", "1 Main St", "Confirmed", 100m, DateTime.UtcNow, DateTime.UtcNow, []);
        _mockOrderService.Setup(s => s.UpdateStatusAsync(1, OrderStatus.Confirmed)).ReturnsAsync(order);
        var result = await _controller.UpdateStatus(1, req);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(order, ok.Value);
    }

    [Fact]
    public async Task Cancel_WhenFound_ReturnsNoContent()
    {
        _mockOrderService.Setup(s => s.CancelAsync(1)).ReturnsAsync(true);
        var result = await _controller.Cancel(1);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Cancel_WhenNotFound_ReturnsNotFound()
    {
        _mockOrderService.Setup(s => s.CancelAsync(99)).ReturnsAsync(false);
        var result = await _controller.Cancel(99);
        Assert.IsType<NotFoundResult>(result);
    }
}
