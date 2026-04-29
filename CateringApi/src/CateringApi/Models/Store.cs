using System.ComponentModel.DataAnnotations;

namespace CateringApi.Models;

public class Store
{
    public int Id { get; set; }
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string Address { get; set; } = string.Empty;
    [MaxLength(100)] public string City { get; set; } = string.Empty;
    [MaxLength(50)] public string Phone { get; set; } = string.Empty;
    [MaxLength(200)] public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
