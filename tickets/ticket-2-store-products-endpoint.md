# Ticket 2 — Add `GET /api/stores/{id}/products` Endpoint

## Bakgrund

Klienter vill kunna bläddra bland alla tillgängliga produkter för en specifik butik utan att behöva filtrera via huvud-endpointen `/api/products`. Vi behöver en dedikerad endpoint på stores-resursen som returnerar de produkter som tillhör en given butik.

Detta ärende berör varje lager i applikationen — DTO, service-interface, service-implementation och controller — vilket gör det till en bra övning i att navigera i en lagerbaserad arkitektur.

## Acceptanskriterier

- `GET /api/stores/{id}/products` returnerar alla produkter där `StoreId == id` och `IsAvailable == true`.
- Om butiken inte finns returnerar endpointen `404 Not Found`.
- Svarets body är en lista av `ProductResponse`-objekt (återanvänd befintlig DTO).
- Endpointen är dokumenterad i Swagger.

---

## Step-by-Step Guide

### Step 1 — Understand the existing data model

Before writing any code, look at the following files to understand the relationships:

- `src/CateringApi/Models/Store.cs` — the `Store` entity
- `src/CateringApi/Models/Product.cs` — the `Product` entity (has a `StoreId` foreign key)
- `src/CateringApi/DTOs/ProductDtos.cs` — the `ProductResponse` record we will reuse
- `src/CateringApi/Services/IStoreService.cs` — the interface we will extend

> **Discussion point:** The `ProductResponse` DTO already exists. Should we create a new DTO for this endpoint or reuse the existing one? What are the trade-offs?

---

### Step 2 — Add the method to `IStoreService`

Open `src/CateringApi/Services/IStoreService.cs`.

Add a new method signature:

```csharp
Task<IEnumerable<ProductResponse>?> GetProductsAsync(int storeId);
```

> The return type is nullable (`?`) — returning `null` signals that the **store itself was not found**, which is different from an empty list (store exists but has no available products).

---

### Step 3 — Implement `GetProductsAsync` in `StoreService`

Open `src/CateringApi/Services/StoreService.cs`.

Add the using directive for the DTOs namespace at the top if it is not already there:

```csharp
using CateringApi.DTOs;
```

Then add the method implementation:

```csharp
public async Task<IEnumerable<ProductResponse>?> GetProductsAsync(int storeId)
{
    var storeExists = await db.Stores.AnyAsync(s => s.Id == storeId);
    if (!storeExists) return null;

    return await db.Products
        .Include(p => p.Category)
        .Where(p => p.StoreId == storeId && p.IsAvailable)
        .Select(p => new ProductResponse(
            p.Id, p.Name, p.Description, p.Price, p.ImageUrl,
            p.CategoryId, p.Category.Name, p.StoreId, p.Store!.Name,
            p.IsAvailable, p.Stock))
        .ToListAsync();
}
```

> **Discussion point:** We use `AnyAsync` to check store existence rather than loading the whole `Store` entity. Why is this more efficient?

---

### Step 4 — Add the endpoint to `StoresController`

Open `src/CateringApi/Controllers/StoresController.cs`.

Add the new action at the bottom of the class. Note the route attribute — this is a **nested resource** route:

```csharp
[HttpGet("{id}/products")]
public async Task<IActionResult> GetProducts(int id)
{
    var result = await storeService.GetProductsAsync(id);
    return result is null ? NotFound() : Ok(result);
}
```

> **Discussion point:** Why is this endpoint on `StoresController` rather than `ProductsController`? Both would work — what guides the decision? (Hint: think about the resource hierarchy in the URL.)

---

### Step 5 — Manual verification

Run the app and open Swagger.

Test the following scenarios:
- `GET /api/stores/1/products` — should return available products for store 1.
- `GET /api/stores/99999/products` — should return `404 Not Found`.
- Create a product with `IsAvailable = false` for store 1, then call the endpoint again — the unavailable product should not appear.

---

## Stretch Goals

- Add an `includeUnavailable` query parameter (`bool`, default `false`) that when set to `true` returns all products regardless of availability. Useful for admin screens.
- Add pagination to this endpoint using the `PagedResponse<T>` wrapper from Ticket 1 (if that ticket has already been completed).
- Write a unit test for `GetProductsAsync` covering: store not found, store found with products, store found with no products.
