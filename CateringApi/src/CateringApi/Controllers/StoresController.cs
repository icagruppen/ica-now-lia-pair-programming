using CateringApi.DTOs;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CateringApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController(IStoreService storeService) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await storeService.GetAllAsync());
    [HttpGet("{id}")] public async Task<IActionResult> GetById(int id)
    {
        var result = await storeService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateStoreRequest request)
    {
        var result = await storeService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    [HttpPut("{id}")] public async Task<IActionResult> Update(int id, [FromBody] UpdateStoreRequest request)
    {
        var result = await storeService.UpdateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id)
    {
        var deleted = await storeService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
