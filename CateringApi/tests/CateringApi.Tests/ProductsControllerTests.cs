using CateringApi.Controllers;
using CateringApi.DTOs;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CateringApi.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockProductService = new();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(_mockProductService.Object);
    }

    private static ProductResponse MakeProduct(int id) =>
        new(id, $"Product {id}", "Desc", 99.99m, "", 1, "Category", 1, "Store", true, 10);

    [Fact]
    public async Task GetAll_ReturnsOkWithProducts()
    {
        var products = new List<ProductResponse> { MakeProduct(1), MakeProduct(2) };
        _mockProductService.Setup(s => s.GetAllAsync(null, null, null)).ReturnsAsync(products);
        var result = await _controller.GetAll(null, null, null);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(products, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var product = MakeProduct(1);
        _mockProductService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);
        var result = await _controller.GetById(1);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(product, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _mockProductService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((ProductResponse?)null);
        var result = await _controller.GetById(99);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var request = new CreateProductRequest("New Product", "Desc", 50m, "", 1, 1);
        var product = MakeProduct(5);
        _mockProductService.Setup(s => s.CreateAsync(request)).ReturnsAsync(product);
        var result = await _controller.Create(request);
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(product, created.Value);
    }

    [Fact]
    public async Task Update_WhenFound_ReturnsOk()
    {
        var request = new UpdateProductRequest("Updated", "Desc", 60m, "", 1, 1);
        var product = MakeProduct(1);
        _mockProductService.Setup(s => s.UpdateAsync(1, request)).ReturnsAsync(product);
        var result = await _controller.Update(1, request);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(product, ok.Value);
    }

    [Fact]
    public async Task Delete_WhenFound_ReturnsNoContent()
    {
        _mockProductService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);
        var result = await _controller.Delete(1);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _mockProductService.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);
        var result = await _controller.Delete(99);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateStock_WhenFound_ReturnsOk()
    {
        var request = new UpdateStockRequest(25);
        var product = MakeProduct(1);
        _mockProductService.Setup(s => s.UpdateStockAsync(1, 25)).ReturnsAsync(product);
        var result = await _controller.UpdateStock(1, request);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(product, ok.Value);
    }

    [Fact]
    public async Task UpdateStock_WhenNotFound_ReturnsNotFound()
    {
        _mockProductService.Setup(s => s.UpdateStockAsync(99, 5)).ReturnsAsync((ProductResponse?)null);
        var result = await _controller.UpdateStock(99, new UpdateStockRequest(5));
        Assert.IsType<NotFoundResult>(result);
    }
}
