using System.ComponentModel.DataAnnotations;

namespace CateringApi.DTOs;

public record ProductResponse(int Id, string Name, string Description, decimal Price, string ImageUrl, int CategoryId, string CategoryName, int StoreId, string StoreName, bool IsAvailable, int Stock);

public record CreateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string Description,
    [Range(0, double.MaxValue)] decimal Price,
    [MaxLength(500)] string ImageUrl,
    [Range(1, int.MaxValue)] int CategoryId,
    [Range(1, int.MaxValue)] int StoreId,
    bool IsAvailable = true,
    int Stock = 0);

public record UpdateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string Description,
    [Range(0, double.MaxValue)] decimal Price,
    [MaxLength(500)] string ImageUrl,
    [Range(1, int.MaxValue)] int CategoryId,
    [Range(1, int.MaxValue)] int StoreId,
    bool IsAvailable = true,
    int Stock = 0);

public record UpdateStockRequest([Range(0, int.MaxValue)] int Stock);
