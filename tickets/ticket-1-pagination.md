## Step-by-Step Guide

### Step 1 — Create the `CursorPagedResponse<T>` DTO

Open `src/CateringApi/DTOs/ProductDtos.cs`.

Add a new generic record **above** the existing product records:

```csharp
public record CursorPagedResponse<T>(IEnumerable<T> Items, int? NextCursor, bool HasNextPage);
```

> **Discussion point:** We no longer include a `totalCount`. Why not? What are the trade-offs of cursor pagination vs. offset pagination? When would you choose one over the other?

---

### Step 2 — Update `IProductService`

Open `src/CateringApi/Services/IProductService.cs`.

Change the signature of `GetAllAsync` to accept the new parameters and return the cursor response:

```csharp
Task<CursorPagedResponse<ProductResponse>> GetAllAsync(
    int? categoryId = null,
    int? storeId = null,
    string? search = null,
    int? after = null,
    int limit = 10);
```

> The interface must be updated first because both the service implementation and the controller depend on it.

---

### Step 3 — Update `ProductService`

Open `src/CateringApi/Services/ProductService.cs`.

Replace the `GetAllAsync` method body with the cursor-based implementation. The key steps are:

1. Build the filtered `IQueryable<Product>` as before.
2. **Always order by `Id` ascending** — cursor pagination requires a stable sort.
3. If `after` has a value, add a `Where(p => p.Id > after)` clause.
4. Fetch **`limit + 1` items** — the extra item tells us whether a next page exists without a separate `COUNT` query.
5. If `limit + 1` items came back, there is a next page: remove the extra item and set `nextCursor` to the last remaining item's `Id`.
6. Return a `CursorPagedResponse<ProductResponse>`.

```csharp
public async Task<CursorPagedResponse<ProductResponse>> GetAllAsync(
    int? categoryId = null,
    int? storeId = null,
    string? search = null,
    int? after = null,
    int limit = 10)
{
    var query = db.Products.Include(p => p.Category).Include(p => p.Store).AsQueryable();

    if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
    if (storeId.HasValue)    query = query.Where(p => p.StoreId == storeId);
    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

    // Stable sort is required for cursor pagination to be consistent
    query = query.OrderBy(p => p.Id);

    // Apply the cursor — only return items after the last seen Id
    if (after.HasValue)
        query = query.Where(p => p.Id > after.Value);

    // Fetch one extra item to detect whether a next page exists
    var rows = await query
        .Take(limit + 1)
        .Select(p => new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.ImageUrl,
            p.CategoryId, p.Category.Name, p.StoreId, p.Store.Name, p.IsAvailable, p.Stock))
        .ToListAsync();

    var hasNextPage = rows.Count > limit;
    var items = hasNextPage ? rows.Take(limit) : rows;
    var nextCursor = hasNextPage ? items.Last().Id : (int?)null;

    return new CursorPagedResponse<ProductResponse>(items, nextCursor, hasNextPage);
}
```

> **Discussion point:** Why do we fetch `limit + 1` rows instead of running a separate `CountAsync()`? What is the performance benefit? Can you think of a scenario where fetching the extra row still isn't enough to know if there's a next page?

---

### Step 4 — Update `ProductsController`

Open `src/CateringApi/Controllers/ProductsController.cs`.

Update the `GetAll` action to accept the new parameters and pass them through:

```csharp
[HttpGet]
public async Task<IActionResult> GetAll(
    [FromQuery] int? categoryId,
    [FromQuery] int? storeId,
    [FromQuery] string? search,
    [FromQuery] int? after,
    [FromQuery] int limit = 10)
    => Ok(await productService.GetAllAsync(categoryId, storeId, search, after, limit));
```

> **Discussion point:** The cursor value here is a plain integer `Id`. In production systems cursors are often opaque (e.g. base64-encoded or encrypted). Why might you want to hide the implementation detail from the client?

---

### Step 5 — Manual verification

Run the app with `dotnet run` and open Swagger at `https://localhost:{port}/swagger`.

Walk through a full pagination sequence:

1. `GET /api/products?limit=5` — note the `nextCursor` value in the response (e.g. `42`).
2. `GET /api/products?limit=5&after=42` — confirm the items start *after* id 42 and a new `nextCursor` is returned.
3. Keep following `nextCursor` until `hasNextPage` is `false` and `nextCursor` is `null`.
4. Try combining with filters: `GET /api/products?search=cake&limit=3&after=10`.

---

## Stretch Goals

- Add input validation: return `400 Bad Request` if `limit` is outside a reasonable range (e.g. 1–100).
- Make the cursor opaque by base64-encoding it before returning it to the client, and decoding it on receipt.
- Apply the same `CursorPagedResponse<T>` pattern to `GET /api/orders`.
