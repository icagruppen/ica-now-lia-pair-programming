using CateringApi.Data;
using CateringApi.DTOs;
using CateringApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringApi.Services;

public class CategoryService(CateringDbContext db, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        return await db.Categories.OrderBy(c => c.SortOrder)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description, c.ImageUrl, c.SortOrder))
            .ToListAsync();
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var c = await db.Categories.FindAsync(id);
        return c is null ? null : new CategoryResponse(c.Id, c.Name, c.Description, c.ImageUrl, c.SortOrder);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var c = new Category { Name = request.Name, Description = request.Description, ImageUrl = request.ImageUrl, SortOrder = request.SortOrder };
        db.Categories.Add(c);
        await db.SaveChangesAsync();
        logger.LogInformation("Created category {CategoryId}: {CategoryName}", c.Id, c.Name);
        return new CategoryResponse(c.Id, c.Name, c.Description, c.ImageUrl, c.SortOrder);
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return null;
        c.Name = request.Name;
        c.Description = request.Description;
        c.ImageUrl = request.ImageUrl;
        c.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated category {CategoryId}", id);
        return new CategoryResponse(c.Id, c.Name, c.Description, c.ImageUrl, c.SortOrder);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return false;
        db.Categories.Remove(c);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted category {CategoryId}", id);
        return true;
    }
}
