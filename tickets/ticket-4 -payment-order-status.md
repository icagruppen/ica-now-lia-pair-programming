# Ticket 4 — Auto-Update Order Status When Payment Completes

## Bakgrund

När en betalning i den nuvarande lösningen behandlas framgångsrikt via `POST /api/payments/{id}/process` markeras betalningsposten som `Completed`, men den länkade orderns status förblir `Pending`. Det innebär att orderlivscykeln är bruten — personalen kan se en betald order som fortfarande ligger i läget "Pending".

Vi behöver att `PaymentService.ProcessAsync` automatiskt övergår till att sätta ordern till `Confirmed` när en betalning lyckas.

## Acceptanskriterier

- När `ProcessAsync` markerar en betalning som `Completed` ska den länkade orderns status uppdateras till `Confirmed`.
- Om ordern inte hittas (ett dataintegritetsproblem) ska metoden logga en varning men **inte** kasta en exception — betalningen har redan sparats och ska inte rullas tillbaka.
- Inga nya endpoints behövs; detta är en förändring av affärslogiken inuti service-lagret.

---

## Step-by-Step Guide

### Step 1 — Understand the current flow

Read the following files before making any changes:

- `src/CateringApi/Services/PaymentService.cs` — specifically `ProcessAsync`
- `src/CateringApi/Services/IOrderService.cs` — look at the `UpdateStatusAsync` method signature
- `src/CateringApi/Models/Order.cs` — the `OrderStatus` enum values

The current `ProcessAsync` method:
1. Loads the payment by id.
2. Sets `Status = PaymentStatus.Completed` and generates a `TransactionId`.
3. Saves to the database.
4. Publishes a `PaymentCompletedEvent` to the Service Bus.

It does **not** touch the order. That is what we are adding.

> **Discussion point:** Look at how `IOrderService` is already registered in `Program.cs`. What DI lifetime is used (`AddScoped`)? Why does that matter here?

---

### Step 2 — Inject `IOrderService` into `PaymentService`

Open `src/CateringApi/Services/PaymentService.cs`.

`PaymentService` currently uses primary constructor injection:

```csharp
public class PaymentService(CateringDbContext db, IServiceBusService serviceBus, ILogger<PaymentService> logger) : IPaymentService
```

Add `IOrderService` as an additional constructor parameter:

```csharp
public class PaymentService(
    CateringDbContext db,
    IServiceBusService serviceBus,
    ILogger<PaymentService> logger,
    IOrderService orderService) : IPaymentService
```

> **Discussion point:** We are injecting a service into another service. Is this a valid pattern? What would be a sign that this has gone too far (circular dependency)?

---

### Step 3 — Update `ProcessAsync` to transition the order status

Still in `PaymentService.cs`, find the `ProcessAsync` method and add the order status update **after** saving the payment but **before** publishing the Service Bus event:

```csharp
public async Task<PaymentResponse?> ProcessAsync(int id)
{
    var p = await db.Payments.FindAsync(id);
    if (p is null) return null;

    p.Status = PaymentStatus.Completed;
    p.TransactionId = $"TXN-{Guid.NewGuid():N}";
    p.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    logger.LogInformation("Processed payment {PaymentId}, transaction {TransactionId}", id, p.TransactionId);

    // Transition the linked order to Confirmed
    var updatedOrder = await orderService.UpdateStatusAsync(p.OrderId, OrderStatus.Confirmed);
    if (updatedOrder is null)
        logger.LogWarning("Payment {PaymentId} completed but linked order {OrderId} was not found", id, p.OrderId);

    await serviceBus.SendAsync(new PaymentCompletedEvent(p.Id, p.OrderId, p.Amount, p.TransactionId, p.UpdatedAt));

    return ToResponse(p);
}
```

> **Discussion point:** Why do we check `if (updatedOrder is null)` and log a warning rather than throwing an exception? Consider: the payment is already saved to the database. What would happen to data consistency if we threw and the caller received a 500?

---

### Step 4 — Add the `OrderStatus` using directive

Because `PaymentService` now references `OrderStatus`, make sure the using directive for the models namespace is present at the top of the file:

```csharp
using CateringApi.Models;
```

---

### Step 5 — Manual verification

Run the app and follow this sequence in Swagger:

1. `POST /api/orders` — create an order; note the `id` returned and confirm `status` is `"Pending"`.
2. `POST /api/payments` — initiate a payment for that order (`orderId` from step 1).
3. `POST /api/payments/{id}/process` — process the payment.
4. `GET /api/orders/{id}` — confirm the order `status` is now `"Confirmed"`.

---

## Stretch Goals

- Write a unit test for `ProcessAsync` that mocks `IOrderService` and asserts `UpdateStatusAsync` is called with the correct `OrderId` and `OrderStatus.Confirmed`.
- Handle the case where `ProcessAsync` is called on a payment that is already `Completed` — should it be idempotent or return an error?
- Consider what should happen to the order status when `RefundAsync` is called — should the order revert to a previous status?
