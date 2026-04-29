namespace CateringApi.Services;

public interface IServiceBusService
{
    Task SendAsync<T>(T eventMessage) where T : class;
}
