using CateringApi.DTOs;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CateringApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] int? categoryId, [FromQuery] int? storeId, [FromQuery] string? search)
        => Ok(await productService.GetAllAsync(categoryId, storeId, search));
    [HttpGet("{id}")] public async Task<IActionResult> GetById(int id)
    {
        var result = await productService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    [HttpPut("{id}")] public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        var result = await productService.UpdateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id)
    {
        var deleted = await productService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
    [HttpPatch("{id}/stock")] public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockRequest request)
    {
        var result = await productService.UpdateStockAsync(id, request.Stock);
        return result is null ? NotFound() : Ok(result);
    }
}
