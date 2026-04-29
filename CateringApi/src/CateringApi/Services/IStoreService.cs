using CateringApi.DTOs;

namespace CateringApi.Services;

public interface IStoreService
{
    Task<IEnumerable<StoreResponse>> GetAllAsync();
    Task<StoreResponse?> GetByIdAsync(int id);
    Task<StoreResponse> CreateAsync(CreateStoreRequest request);
    Task<StoreResponse?> UpdateAsync(int id, UpdateStoreRequest request);
    Task<bool> DeleteAsync(int id);
}
