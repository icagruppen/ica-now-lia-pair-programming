using System.ComponentModel.DataAnnotations;

namespace CateringApi.Models;

public class Product
{
    public int Id { get; set; }
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(1000)] public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    [MaxLength(500)] public string ImageUrl { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public bool IsAvailable { get; set; } = true;
    public int Stock { get; set; }
}
