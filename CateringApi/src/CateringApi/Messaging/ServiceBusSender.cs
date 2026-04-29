using CateringApi.Messaging.Events;
using CateringApi.Services;

namespace CateringApi.Messaging;

public class ServiceBusSender(IServiceBusService serviceBusService, ILogger<ServiceBusSender> logger)
{
    public async Task SendOrderCreatedAsync(OrderCreatedEvent evt)
    {
        logger.LogInformation("Sending OrderCreatedEvent for order {OrderId}", evt.OrderId);
        await serviceBusService.SendAsync(evt);
    }

    public async Task SendPaymentCompletedAsync(PaymentCompletedEvent evt)
    {
        logger.LogInformation("Sending PaymentCompletedEvent for payment {PaymentId}", evt.PaymentId);
        await serviceBusService.SendAsync(evt);
    }

    public async Task SendOrderStatusChangedAsync(OrderStatusChangedEvent evt)
    {
        logger.LogInformation("Sending OrderStatusChangedEvent for order {OrderId}", evt.OrderId);
        await serviceBusService.SendAsync(evt);
    }
}
