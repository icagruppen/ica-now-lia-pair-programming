using CateringApi.Data;
using CateringApi.DTOs;
using CateringApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringApi.Services;

public class ProductService(CateringDbContext db, ILogger<ProductService> logger) : IProductService
{
    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.ImageUrl, p.CategoryId, p.Category?.Name ?? "", p.StoreId, p.Store?.Name ?? "", p.IsAvailable, p.Stock);

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(int? categoryId = null, int? storeId = null, string? search = null)
    {
        var query = db.Products.Include(p => p.Category).Include(p => p.Store).AsQueryable();
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        if (storeId.HasValue) query = query.Where(p => p.StoreId == storeId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        return await query.Select(p => new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.ImageUrl, p.CategoryId, p.Category.Name, p.StoreId, p.Store.Name, p.IsAvailable, p.Stock)).ToListAsync();
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var p = await db.Products.Include(p => p.Category).Include(p => p.Store).FirstOrDefaultAsync(p => p.Id == id);
        return p is null ? null : ToResponse(p);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var p = new Product { Name = request.Name, Description = request.Description, Price = request.Price, ImageUrl = request.ImageUrl, CategoryId = request.CategoryId, StoreId = request.StoreId, IsAvailable = request.IsAvailable, Stock = request.Stock };
        db.Products.Add(p);
        await db.SaveChangesAsync();
        await db.Entry(p).Reference(x => x.Category).LoadAsync();
        await db.Entry(p).Reference(x => x.Store).LoadAsync();
        logger.LogInformation("Created product {ProductId}: {ProductName}", p.Id, p.Name);
        return ToResponse(p);
    }

    public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request)
    {
        var p = await db.Products.Include(p => p.Category).Include(p => p.Store).FirstOrDefaultAsync(p => p.Id == id);
        if (p is null) return null;
        p.Name = request.Name; p.Description = request.Description; p.Price = request.Price;
        p.ImageUrl = request.ImageUrl; p.CategoryId = request.CategoryId; p.StoreId = request.StoreId;
        p.IsAvailable = request.IsAvailable; p.Stock = request.Stock;
        await db.SaveChangesAsync();
        await db.Entry(p).Reference(x => x.Category).LoadAsync();
        await db.Entry(p).Reference(x => x.Store).LoadAsync();
        logger.LogInformation("Updated product {ProductId}", id);
        return ToResponse(p);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return false;
        db.Products.Remove(p);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted product {ProductId}", id);
        return true;
    }

    public async Task<ProductResponse?> UpdateStockAsync(int id, int stock)
    {
        var p = await db.Products.Include(p => p.Category).Include(p => p.Store).FirstOrDefaultAsync(p => p.Id == id);
        if (p is null) return null;
        p.Stock = stock;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated stock for product {ProductId} to {Stock}", id, stock);
        return ToResponse(p);
    }
}
