# Ticket 3 — Implement Global Exception Handling Middleware

## Bakgrund

Om en exception i den nuvarande lösningen kastas utan att fångas returnerar ASP.NET-runtimen ett 500-svar med en HTML-felsida avsedd för utvecklare. I ett riktigt API förväntar sig klienter ett konsekvent JSON-felformat oavsett vad som gick fel. Vi vill lägga till ett middleware som fångar upp alla ohanterade exceptions och returnerar ett strukturerat `ProblemDetails`-svar.

## Acceptanskriterier

- Alla ohanterade exceptions ger ett JSON-svar i `ProblemDetails`-format (RFC 7807).
- En `NotFoundException` (en egen exception som vi skapar) mappas till HTTP `404 Not Found`.
- Alla andra oväntade exceptions mappas till HTTP `500 Internal Server Error`.
- Undantagets meddelande och stack trace **exponeras aldrig** till klienten i produktion.
- Varje fångad exception loggas med den injicerade `ILogger`.

---

## Step-by-Step Guide

### Step 1 — Create a `NotFoundException`

Create a new file `src/CateringApi/Exceptions/NotFoundException.cs`:

```csharp
namespace CateringApi.Exceptions;

public class NotFoundException(string message) : Exception(message);
```

> **Discussion point:** Why do we create a dedicated exception type rather than returning `null` from the service? What does it communicate to the caller?

---

### Step 2 — Create the middleware class

Create a new file `src/CateringApi/Middleware/ExceptionHandlingMiddleware.cs`:

```csharp
using System.Net;
using System.Text.Json;
using CateringApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CateringApi.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
```

> **Discussion point:** What does `RequestDelegate next` represent? Draw the middleware pipeline on a whiteboard — what happens when `next(context)` is called?

---

### Step 3 — Register the middleware in `Program.cs`

Open `src/CateringApi/Program.cs`.

Add the middleware **early** in the pipeline, before `app.UseCors()` and `app.MapControllers()`:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

> **Discussion point:** Order matters in the middleware pipeline. Why do we place this *before* `UseCors()` and `MapControllers()`? What would happen if it were placed *after*?

---

### Step 4 — Use `NotFoundException` in a service (optional but recommended)

To test that the middleware works end-to-end, update one service method to throw `NotFoundException` instead of returning `null`.

For example, in `src/CateringApi/Services/ProductService.cs`, update `GetByIdAsync`:

```csharp
public async Task<ProductResponse> GetByIdAsync(int id)
{
    var p = await db.Products.Include(p => p.Category).Include(p => p.Store)
        .FirstOrDefaultAsync(p => p.Id == id);
    if (p is null) throw new NotFoundException($"Product with id {id} was not found.");
    return ToResponse(p);
}
```

> Note: if you do this, the return type changes from `ProductResponse?` to `ProductResponse` (non-nullable) and the controller no longer needs the `result is null ? NotFound() : Ok(result)` guard. Update both `IProductService` and `ProductsController.GetById` accordingly.

---

### Step 5 — Manual verification

Run the app and use Swagger or the `.http` file to call:

- `GET /api/products/99999` — should return a `404` with a JSON `ProblemDetails` body.
- Temporarily throw a `new Exception("boom")` inside a service method, call the endpoint, and confirm you get a `500` with no stack trace exposed.

---

## Stretch Goals

- Add a `ValidationException` that maps to `400 Bad Request` and include the validation errors in the `ProblemDetails.Extensions` dictionary.
- Hide the `Detail` field (the raw exception message) when `app.Environment.IsProduction()` is true — only log it, never send it to the client.
- Write a unit test that directly calls `ExceptionHandlingMiddleware.InvokeAsync` with a mock `RequestDelegate` that throws, and asserts the response status code and content type.
