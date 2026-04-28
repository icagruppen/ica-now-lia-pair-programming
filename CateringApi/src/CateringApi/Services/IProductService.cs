using CateringApi.DTOs;

namespace CateringApi.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllAsync(int? categoryId = null, int? storeId = null, string? search = null);
    Task<ProductResponse?> GetByIdAsync(int id);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id);
    Task<ProductResponse?> UpdateStockAsync(int id, int stock);
}
