# EasyServiceRegister

**Simple, Attribute-Based Dependency Injection for .NET**

EasyServiceRegister is a lightweight library built on top of `Microsoft.Extensions.DependencyInjection.Abstractions`. It simplifies service registration using attributes — supporting all lifetimes, keyed services (.NET 8+), the decorator pattern, startup validation, and registration diagnostics.

**Targets:** `netstandard2.1` · `net8.0` · `net9.0`

---

## Installation

```bash
dotnet add package EasyServiceRegister
```

---

## Quick Start

```csharp
// 1. Decorate your services
[RegisterAsScoped]
public class ProductService : IProductService
{
    public Task<Product> CreateProduct(Product product) { /* ... */ }
}

// 2. Register and validate at startup
builder.Services
    .AddServices(typeof(Program))
    .EnsureServicesAreValid();
```

That's it. No manual `services.AddScoped<...>()` calls needed.

---

## Registration Attributes

| Attribute | Lifetime | Keyed |
|-----------|----------|-------|
| `[RegisterAsSingleton]` | Singleton | No |
| `[RegisterAsScoped]` | Scoped | No |
| `[RegisterAsTransient]` | Transient | No |
| `[RegisterAsSingletonKeyed("key")]` | Singleton | Yes (.NET 8+) |
| `[RegisterAsScopedKeyed("key")]` | Scoped | Yes (.NET 8+) |
| `[RegisterAsTransientKeyed("key")]` | Transient | Yes (.NET 8+) |

### Attribute Parameters

All registration attributes share these optional parameters:

| Parameter | Default | Description |
|-----------|---------|-------------|
| `serviceInterface` | `null` | Specific interface to register the service as |
| `useTryAdd*` | `false` | Use `TryAdd` instead of `Add` (won't override existing registrations) |
| `registerAsAllInterfaces` | `true` | Register the service against all implemented interfaces |

Keyed attributes also require a `key` parameter (any `object`: string, enum, etc.).

### Specifying a Service Interface

When a class implements multiple interfaces, you can target a specific one:

```csharp
[RegisterAsScoped(serviceInterface: typeof(IOrderService))]
public class OrderService : IOrderService, IDisposable
{
    // Registered only as IOrderService
}
```

### Registering as All Interfaces

```csharp
[RegisterAsScoped]
public class NotificationService : IEmailNotifier, ISmsNotifier
{
    // Registered as both IEmailNotifier and ISmsNotifier
}
```

### Open Generic Support

```csharp
[RegisterAsScoped]
public class Repository<T> : IRepository<T>
{
    // Registered as IRepository<> (open generic)
}
```

### Using TryAdd

```csharp
[RegisterAsSingleton(useTryAddSingleton: true)]
public class CacheProvider : ICacheProvider
{
    // Only registered if ICacheProvider isn't already in the container
}
```

---

## Scanning and Registration

Call `AddServices` in your `Program.cs` or `Startup.cs`:

```csharp
// Scan one or more assemblies via marker types
services.AddServices(typeof(Program));
services.AddServices(typeof(MarkerA), typeof(MarkerB));

// Or pass assemblies directly
services.AddServices(typeof(MarkerA).Assembly);
```

### Filtering Types

```csharp
services.AddServices(
    filter: type => type.Namespace.Contains("Services"),
    typeof(Program));
```

The filter receives a `TypeInfo` and returns `false` to skip registration.

---

## Keyed Services (.NET 8+)

```csharp
[RegisterAsSingletonKeyed("primary")]
public class PrimaryEmailService : IEmailService { }

[RegisterAsSingletonKeyed("secondary")]
public class SecondaryEmailService : IEmailService { }
```

Resolve by key using `[FromKeyedServices]`:

```csharp
public class EmailManager
{
    public EmailManager(
        [FromKeyedServices("primary")] IEmailService primary,
        [FromKeyedServices("secondary")] IEmailService secondary)
    { }
}
```

---

## Decorator Pattern

Layer cross-cutting concerns using `[DecorateWith]`. Multiple decorators can be applied and are ordered by the `order` parameter (lower values wrap first):

```csharp
[RegisterAsScoped]
[DecorateWith(typeof(LoggingDecorator), order: 0)]
[DecorateWith(typeof(CachingDecorator), order: 1)]
public class ProductService : IProductService
{
    public Product GetById(int id) { /* ... */ }
}
```

Each decorator must implement the same interface and receive the inner service through its constructor:

```csharp
public class LoggingDecorator : IProductService
{
    private readonly IProductService _inner;
    private readonly ILogger<LoggingDecorator> _logger;

    public LoggingDecorator(IProductService inner, ILogger<LoggingDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public Product GetById(int id)
    {
        _logger.LogInformation("Getting product {Id}", id);
        return _inner.GetById(id);
    }
}
```

Resolution chain: `LoggingDecorator` → `CachingDecorator` → `ProductService`

---

## Startup Validation

EasyServiceRegister includes a validation system that detects common DI misconfigurations at startup — before they become runtime exceptions.

Validation runs against **all services in the container**, not just those registered through EasyServiceRegister. Framework-internal services (types under `System.*` and `Microsoft.*` namespaces) are automatically excluded to avoid false positives.

### Setup

```csharp
builder.Services
    .AddServices(typeof(Program))
    .EnsureServicesAreValid(); // Throws ServiceValidationException on errors
```

`EnsureServicesAreValid()` throws a `ServiceValidationException` if any **errors** are detected. Warnings are included in the exception details but don't trigger a throw on their own. The method returns `IServiceCollection` for chaining.

### What It Detects

| Issue | Severity | Description |
|-------|----------|-------------|
| **Missing dependencies** | Error | A service's constructor requires a type that isn't registered in the container |
| **Scoped in singleton** | Error | A singleton depends on a scoped service — the DI container will throw at runtime |
| **Captive dependency chain** | Error | A singleton → singleton → scoped chain silently captures a scoped service |
| **Circular dependencies** | Error | Cyclic dependency graphs (A → B → C → A) |
| **Disposable transients** | Warning | A transient implements `IDisposable` but won't be disposed by the container, causing potential memory/resource leaks |
| **Transient in singleton** | Warning | A singleton depends on a transient — the transient is instantiated only once and reused, losing its intended lifetime semantics |

### Advanced Usage

For programmatic access without throwing, use `ValidateServices()` directly:

```csharp
var issues = builder.Services.ValidateServices();

foreach (var issue in issues)
{
    Console.WriteLine($"[{issue.Severity}] {issue.Message}");
    // issue.ServiceType and issue.ImplementationType available for programmatic handling
}
```

Filter by severity:

```csharp
var errorsOnly = builder.Services.ValidateServices(minimumSeverity: ValidationSeverity.Error);
```

### Handling the Exception

```csharp
try
{
    builder.Services.EnsureServicesAreValid();
}
catch (ServiceValidationException ex)
{
    foreach (var issue in ex.Issues)
    {
        Console.WriteLine($"[{issue.Severity}] {issue.Message}");
    }
}
```

---

## Registration Diagnostics

Query what was registered through EasyServiceRegister at runtime:

```csharp
var all = ServicesExtension.GetRegisteredServices();

foreach (var svc in all)
{
    Console.WriteLine($"{svc.ServiceType.Name} → {svc.ImplementationType.Name} [{svc.Lifetime}]");
    Console.WriteLine($"  Method: {svc.RegistrationMethod}, Attribute: {svc.AttributeUsed}, Key: {svc.ServiceKey}");

    foreach (var dec in svc.Decorators)
    {
        Console.WriteLine($"  Decorator: {dec.DecoratorType.Name} (order: {dec.Order})");
    }
}
```

Filter by type or lifetime:

```csharp
var singletons = ServicesExtension.GetRegisteredServices(lifetime: ServiceLifetime.Singleton);
var specific = ServicesExtension.GetRegisteredServices(serviceType: typeof(IProductService));
```

Clear the log (useful in test scenarios):

```csharp
ServicesExtension.ClearRegistrationLog();
```

---

## License

MIT ©
