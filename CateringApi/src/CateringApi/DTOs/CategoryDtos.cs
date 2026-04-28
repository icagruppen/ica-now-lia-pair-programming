using System.ComponentModel.DataAnnotations;

namespace CateringApi.DTOs;

public record CategoryResponse(int Id, string Name, string Description, string ImageUrl, int SortOrder);

public record CreateCategoryRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(500)] string Description,
    [MaxLength(500)] string ImageUrl,
    int SortOrder = 0);

public record UpdateCategoryRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(500)] string Description,
    [MaxLength(500)] string ImageUrl,
    int SortOrder = 0);
