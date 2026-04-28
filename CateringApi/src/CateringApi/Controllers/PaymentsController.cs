using CateringApi.DTOs;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CateringApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await paymentService.GetAllAsync());
    [HttpGet("{id}")] public async Task<IActionResult> GetById(int id)
    {
        var result = await paymentService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpGet("order/{orderId}")] public async Task<IActionResult> GetByOrder(int orderId)
    {
        var result = await paymentService.GetByOrderIdAsync(orderId);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPost] public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest request)
    {
        try
        {
            var result = await paymentService.InitiateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }
    [HttpPost("{id}/process")] public async Task<IActionResult> Process(int id)
    {
        var result = await paymentService.ProcessAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPost("{id}/refund")] public async Task<IActionResult> Refund(int id)
    {
        var result = await paymentService.RefundAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
