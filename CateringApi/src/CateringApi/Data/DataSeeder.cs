using CateringApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(CateringDbContext db)
    {
        if (await db.Stores.AnyAsync()) return;

        var stores = new List<Store>
        {
            new() { Name = "Downtown Catering Hub", Address = "12 Market Street", City = "Stockholm", Phone = "+46-8-111-2233", Email = "downtown@cateringhub.local", IsActive = true },
            new() { Name = "Westside Kitchen", Address = "45 Sunset Boulevard", City = "Gothenburg", Phone = "+46-31-222-3344", Email = "westside@cateringhub.local", IsActive = true },
            new() { Name = "Northgate Deli", Address = "7 Nordic Avenue", City = "Malmö", Phone = "+46-40-333-4455", Email = "northgate@cateringhub.local", IsActive = true }
        };
        db.Stores.AddRange(stores);
        await db.SaveChangesAsync();

        var categories = new List<Category>
        {
            new() { Name = "Starters", Description = "Delicious appetizers to begin your meal", SortOrder = 1 },
            new() { Name = "Main Course", Description = "Hearty main dishes for every occasion", SortOrder = 2 },
            new() { Name = "Desserts", Description = "Sweet treats to end on a high note", SortOrder = 3 },
            new() { Name = "Beverages", Description = "Refreshing drinks and juices", SortOrder = 4 },
            new() { Name = "Catering Packages", Description = "Complete packages for events and gatherings", SortOrder = 5 }
        };
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        var products = new List<Product>
        {
            // Starters
            new() { Name = "Bruschetta Platter", Description = "Toasted bread topped with tomato, basil and garlic", Price = 89.00m, CategoryId = categories[0].Id, StoreId = stores[0].Id, IsAvailable = true, Stock = 50 },
            new() { Name = "Shrimp Cocktail", Description = "Chilled shrimp with house cocktail sauce", Price = 149.00m, CategoryId = categories[0].Id, StoreId = stores[1].Id, IsAvailable = true, Stock = 30 },
            new() { Name = "Cheese & Charcuterie Board", Description = "Selection of local cheeses and cured meats", Price = 199.00m, CategoryId = categories[0].Id, StoreId = stores[2].Id, IsAvailable = true, Stock = 20 },
            // Main Course
            new() { Name = "Grilled Salmon Fillet", Description = "Atlantic salmon with lemon butter and seasonal veg", Price = 245.00m, CategoryId = categories[1].Id, StoreId = stores[0].Id, IsAvailable = true, Stock = 40 },
            new() { Name = "Beef Tenderloin", Description = "Premium beef with roasted potatoes and red wine jus", Price = 325.00m, CategoryId = categories[1].Id, StoreId = stores[1].Id, IsAvailable = true, Stock = 25 },
            new() { Name = "Vegetarian Pasta Primavera", Description = "Fresh pasta with seasonal vegetables in a light cream sauce", Price = 185.00m, CategoryId = categories[1].Id, StoreId = stores[2].Id, IsAvailable = true, Stock = 60 },
            new() { Name = "Chicken Tikka Masala", Description = "Tender chicken in a rich spiced tomato sauce with rice", Price = 215.00m, CategoryId = categories[1].Id, StoreId = stores[0].Id, IsAvailable = true, Stock = 35 },
            // Desserts
            new() { Name = "Chocolate Lava Cake", Description = "Warm chocolate cake with a gooey center, served with vanilla ice cream", Price = 95.00m, CategoryId = categories[2].Id, StoreId = stores[1].Id, IsAvailable = true, Stock = 45 },
            new() { Name = "Tiramisu", Description = "Classic Italian dessert with mascarpone and espresso", Price = 85.00m, CategoryId = categories[2].Id, StoreId = stores[2].Id, IsAvailable = true, Stock = 50 },
            new() { Name = "Fresh Fruit Pavlova", Description = "Crispy meringue topped with cream and seasonal fruits", Price = 79.00m, CategoryId = categories[2].Id, StoreId = stores[0].Id, IsAvailable = true, Stock = 30 },
            // Beverages
            new() { Name = "Sparkling Water (1L)", Description = "Premium sparkling mineral water", Price = 29.00m, CategoryId = categories[3].Id, StoreId = stores[1].Id, IsAvailable = true, Stock = 200 },
            new() { Name = "Fresh Juice Selection", Description = "Orange, apple or grapefruit - freshly squeezed", Price = 49.00m, CategoryId = categories[3].Id, StoreId = stores[2].Id, IsAvailable = true, Stock = 100 },
            // Catering Packages
            new() { Name = "Business Lunch Package", Description = "Starter, main and beverage for up to 10 guests", Price = 1990.00m, CategoryId = categories[4].Id, StoreId = stores[0].Id, IsAvailable = true, Stock = 10 },
            new() { Name = "Wedding Reception Package", Description = "Full catering for up to 50 guests including 3 courses", Price = 12500.00m, CategoryId = categories[4].Id, StoreId = stores[1].Id, IsAvailable = true, Stock = 5 },
            new() { Name = "Corporate Event Package", Description = "Canapés and drinks for up to 30 guests", Price = 5500.00m, CategoryId = categories[4].Id, StoreId = stores[2].Id, IsAvailable = true, Stock = 8 }
        };
        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}
