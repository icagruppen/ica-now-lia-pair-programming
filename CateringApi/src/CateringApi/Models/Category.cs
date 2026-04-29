using System.ComponentModel.DataAnnotations;

namespace CateringApi.Models;

public class Category
{
    public int Id { get; set; }
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string Description { get; set; } = string.Empty;
    [MaxLength(500)] public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
