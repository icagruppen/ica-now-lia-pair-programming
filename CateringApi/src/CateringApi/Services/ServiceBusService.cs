using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace CateringApi.Services;

public class ServiceBusService : IServiceBusService, IAsyncDisposable
{
    private readonly ServiceBusClient? _client;
    private readonly ILogger<ServiceBusService> _logger;
    private readonly IConfiguration _configuration;

    public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        var connectionString = configuration["AzureServiceBus:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            _client = new ServiceBusClient(connectionString);
            _logger.LogInformation("Azure Service Bus connected");
        }
        else
        {
            _logger.LogWarning("Azure Service Bus connection string not configured - messaging disabled");
        }
    }

    public async Task SendAsync<T>(T eventMessage) where T : class
    {
        if (_client is null) { _logger.LogDebug("Service Bus not configured, skipping event {EventType}", typeof(T).Name); return; }
        var topicName = GetTopicName<T>();
        try
        {
            var sender = _client.CreateSender(topicName);
            var json = JsonSerializer.Serialize(eventMessage);
            await sender.SendMessageAsync(new ServiceBusMessage(json) { ContentType = "application/json" });
            _logger.LogInformation("Sent {EventType} to topic {TopicName}", typeof(T).Name, topicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send {EventType} to Service Bus", typeof(T).Name);
        }
    }

    private string GetTopicName<T>()
    {
        var typeName = typeof(T).Name;
        return typeName switch
        {
            "OrderCreatedEvent" => _configuration["AzureServiceBus:OrderCreatedTopic"] ?? "order-created",
            "PaymentCompletedEvent" => _configuration["AzureServiceBus:PaymentCompletedTopic"] ?? "payment-completed",
            "OrderStatusChangedEvent" => _configuration["AzureServiceBus:OrderStatusChangedTopic"] ?? "order-status-changed",
            _ => typeName.ToLowerInvariant()
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null) await _client.DisposeAsync();
    }
}
