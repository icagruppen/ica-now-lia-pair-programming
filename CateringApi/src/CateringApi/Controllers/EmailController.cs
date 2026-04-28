using CateringApi.DTOs;
using CateringApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CateringApi.Controllers;

[ApiController]
[Route("api/email")]
public class EmailController(IEmailService emailService) : ControllerBase
{
    [HttpPost("send")] public async Task<IActionResult> Send([FromBody] EmailRequest request)
    {
        await emailService.SendAsync(request.To, request.Subject, request.Body);
        return Ok(new { message = "Email queued" });
    }
    [HttpPost("order-confirmation/{orderId}")] public async Task<IActionResult> SendOrderConfirmation(int orderId)
    {
        await emailService.SendOrderConfirmationAsync(orderId);
        return Ok(new { message = "Order confirmation email queued" });
    }
    [HttpPost("payment-receipt/{paymentId}")] public async Task<IActionResult> SendPaymentReceipt(int paymentId)
    {
        await emailService.SendPaymentReceiptAsync(paymentId);
        return Ok(new { message = "Payment receipt email queued" });
    }
}
