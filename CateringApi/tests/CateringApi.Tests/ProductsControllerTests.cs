using CateringApi.Controllers;
using CateringApi.DTOs;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace CateringApi.Tests;

public class ProductsControllerTests
{
    private readonly IProductService _mockProductService = Substitute.For<IProductService>();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(_mockProductService);
    }

    private static ProductResponse MakeProduct(int id) =>
        new(id, $"Product {id}", "Desc", 99.99m, "", 1, "Category", 1, "Store", true, 10);

    [Test]
    public async Task GetAll_ReturnsOkWithProducts()
    {
        var products = new List<ProductResponse> { MakeProduct(1), MakeProduct(2) };
        _mockProductService.GetAllAsync(null, null, null).Returns(products);
        var result = await _controller.GetAll(null, null, null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(products));
    }

    [Test]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var product = MakeProduct(1);
        _mockProductService.GetByIdAsync(1).Returns(product);
        var result = await _controller.GetById(1);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(product));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _mockProductService.GetByIdAsync(99).Returns((ProductResponse?)null);
        var result = await _controller.GetById(99);
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_ReturnsCreated()
    {
        var request = new CreateProductRequest("New Product", "Desc", 50m, "", 1, 1);
        var product = MakeProduct(5);
        _mockProductService.CreateAsync(request).Returns(product);
        var result = await _controller.Create(request);
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        var created = (CreatedAtActionResult)result;
        Assert.That(created.Value, Is.EqualTo(product));
    }

    [Test]
    public async Task Update_WhenFound_ReturnsOk()
    {
        var request = new UpdateProductRequest("Updated", "Desc", 60m, "", 1, 1);
        var product = MakeProduct(1);
        _mockProductService.UpdateAsync(1, request).Returns(product);
        var result = await _controller.Update(1, request);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(product));
    }

    [Test]
    public async Task Delete_WhenFound_ReturnsNoContent()
    {
        _mockProductService.DeleteAsync(1).Returns(true);
        var result = await _controller.Delete(1);
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _mockProductService.DeleteAsync(99).Returns(false);
        var result = await _controller.Delete(99);
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateStock_WhenFound_ReturnsOk()
    {
        var request = new UpdateStockRequest(25);
        var product = MakeProduct(1);
        _mockProductService.UpdateStockAsync(1, 25).Returns(product);
        var result = await _controller.UpdateStock(1, request);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(product));
    }

    [Test]
    public async Task UpdateStock_WhenNotFound_ReturnsNotFound()
    {
        _mockProductService.UpdateStockAsync(99, 5).Returns((ProductResponse?)null);
        var result = await _controller.UpdateStock(99, new UpdateStockRequest(5));
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
