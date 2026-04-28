using CateringApi.DTOs;
using CateringApi.Models;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CateringApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] OrderStatus? status) => Ok(await orderService.GetAllAsync(status));
    [HttpGet("{id}")] public async Task<IActionResult> GetById(int id)
    {
        var result = await orderService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpGet("number/{orderNumber}")] public async Task<IActionResult> GetByOrderNumber(string orderNumber)
    {
        var result = await orderService.GetByOrderNumberAsync(orderNumber);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var result = await orderService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    [HttpPut("{id}")] public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderRequest request)
    {
        var result = await orderService.UpdateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPatch("{id}/status")] public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await orderService.UpdateStatusAsync(id, request.Status);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpDelete("{id}")] public async Task<IActionResult> Cancel(int id)
    {
        var cancelled = await orderService.CancelAsync(id);
        return cancelled ? NoContent() : NotFound();
    }
}
