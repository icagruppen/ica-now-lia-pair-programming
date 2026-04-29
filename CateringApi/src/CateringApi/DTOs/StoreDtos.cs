using System.ComponentModel.DataAnnotations;

namespace CateringApi.DTOs;

public record StoreResponse(int Id, string Name, string Address, string City, string Phone, string Email, bool IsActive);

public record CreateStoreRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(500)] string Address,
    [MaxLength(100)] string City,
    [MaxLength(50)] string Phone,
    [MaxLength(200)] string Email,
    bool IsActive = true);

public record UpdateStoreRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(500)] string Address,
    [MaxLength(100)] string City,
    [MaxLength(50)] string Phone,
    [MaxLength(200)] string Email,
    bool IsActive = true);
