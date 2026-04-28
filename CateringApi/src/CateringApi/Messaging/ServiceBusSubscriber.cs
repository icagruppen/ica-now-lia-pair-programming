using Azure.Messaging.ServiceBus;

namespace CateringApi.Messaging;

public class ServiceBusSubscriber(IConfiguration configuration, ILogger<ServiceBusSubscriber> logger) : BackgroundService
{
    private ServiceBusClient? _client;
    private readonly List<ServiceBusProcessor> _processors = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionString = configuration["AzureServiceBus:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning("Service Bus not configured, subscriber inactive");
            return;
        }

        _client = new ServiceBusClient(connectionString);

        var topics = new[]
        {
            (configuration["AzureServiceBus:OrderCreatedTopic"] ?? "order-created", "catering-api-sub"),
            (configuration["AzureServiceBus:PaymentCompletedTopic"] ?? "payment-completed", "catering-api-sub"),
            (configuration["AzureServiceBus:OrderStatusChangedTopic"] ?? "order-status-changed", "catering-api-sub")
        };

        foreach (var (topic, subscription) in topics)
        {
            var processor = _client.CreateProcessor(topic, subscription);
            processor.ProcessMessageAsync += HandleMessageAsync;
            processor.ProcessErrorAsync += HandleErrorAsync;
            _processors.Add(processor);
            await processor.StartProcessingAsync(stoppingToken);
        }

        logger.LogInformation("Service Bus subscriber started on {Count} topics", _processors.Count);
        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
    }

    private Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        var subject = args.Message.Subject ?? args.EntityPath;
        logger.LogInformation("Received Service Bus message on {Topic}: {Body}", subject, body);
        return args.CompleteMessageAsync(args.Message);
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Service Bus error on {EntityPath}", args.EntityPath);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var processor in _processors)
            await processor.StopProcessingAsync(cancellationToken);
        if (_client is not null) await _client.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
