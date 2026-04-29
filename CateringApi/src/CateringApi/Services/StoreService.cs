using CateringApi.Data;
using CateringApi.DTOs;
using CateringApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringApi.Services;

public class StoreService(CateringDbContext db, ILogger<StoreService> logger) : IStoreService
{
    public async Task<IEnumerable<StoreResponse>> GetAllAsync()
    {
        return await db.Stores
            .Select(s => new StoreResponse(s.Id, s.Name, s.Address, s.City, s.Phone, s.Email, s.IsActive))
            .ToListAsync();
    }

    public async Task<StoreResponse?> GetByIdAsync(int id)
    {
        var store = await db.Stores.FindAsync(id);
        return store is null ? null : new StoreResponse(store.Id, store.Name, store.Address, store.City, store.Phone, store.Email, store.IsActive);
    }

    public async Task<StoreResponse> CreateAsync(CreateStoreRequest request)
    {
        var store = new Store { Name = request.Name, Address = request.Address, City = request.City, Phone = request.Phone, Email = request.Email, IsActive = request.IsActive };
        db.Stores.Add(store);
        await db.SaveChangesAsync();
        logger.LogInformation("Created store {StoreId}: {StoreName}", store.Id, store.Name);
        return new StoreResponse(store.Id, store.Name, store.Address, store.City, store.Phone, store.Email, store.IsActive);
    }

    public async Task<StoreResponse?> UpdateAsync(int id, UpdateStoreRequest request)
    {
        var store = await db.Stores.FindAsync(id);
        if (store is null) return null;
        store.Name = request.Name;
        store.Address = request.Address;
        store.City = request.City;
        store.Phone = request.Phone;
        store.Email = request.Email;
        store.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated store {StoreId}", id);
        return new StoreResponse(store.Id, store.Name, store.Address, store.City, store.Phone, store.Email, store.IsActive);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var store = await db.Stores.FindAsync(id);
        if (store is null) return false;
        db.Stores.Remove(store);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted store {StoreId}", id);
        return true;
    }
}
