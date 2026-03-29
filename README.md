# EasyServiceRegister

**Simple, Attribute-Based Dependency Injection for .NET**

EasyServiceRegister is a lightweight library built on top of `Microsoft.Extensions.DependencyInjection.Abstractions`. It simplifies the registration of services using attributes, supporting lifetimes, keyed services, decorators, and validation — all with minimal boilerplate.

> ✅ Now supporting **keyed services** (.NET 8+), **registration diagnostics**, and **startup validation** in **v3.1.0+**

---

## 🚀 Installation

Install via NuGet:

```bash
dotnet add package EasyServiceRegister
```

---

## ⚙️ How It Works

### 1. Add a registration attribute to your service class

| Attribute | Description |
|----------|-------------|
| `[RegisterAsSingleton]` | Registers as a singleton |
| `[RegisterAsScoped]` | Registers as scoped |
| `[RegisterAsTransient]` | Registers as transient |
| `[RegisterAsSingletonKeyed("key")]` | Singleton with a key (requires .NET 8+) |
| `[RegisterAsScopedKeyed("key")]` | Scoped with a key |
| `[RegisterAsTransientKeyed("key")]` | Transient with a key |

Optional parameters like `useTryAddSingleton`, `useTryAddScoped`, or `useTryAddTransient` let you control whether to use `TryAdd` or `Add`.

If your service implements multiple interfaces, you can specify the intended one using `serviceInterface`:

```csharp
[RegisterAsScoped(serviceInterface: typeof(ISomeService))]
public class SomeService : ISomeService, IDisposable
{
    // ...
}
```

Or register against all implemented interfaces at once:

```csharp
[RegisterAsScoped(registerAsAllInterfaces: true)]
public class SomeService : ISomeService, IAnotherService
{
    // Registered as both ISomeService and IAnotherService
}
```

---

### 2. Register All Services

Call `AddServices` in your `Startup.cs` or `Program.cs`:

```csharp
services.AddServices(typeof(MyServiceMarkerType)); // Marker class from your assembly

// or

services.AddServices(typeof(Assembly1), typeof(Assembly2));
```

You can also filter which types get registered:

```csharp
services.AddServices(
    filter: t => t.Namespace.Contains("Services"),
    typeof(MyServiceMarkerType));
```

---

### 3. Validate at Startup

Add one line to fail fast on misconfigured services:

```csharp
builder.Services.AddServices(typeof(Program));
builder.Services.EnsureServicesAreValid(); // Throws on errors, includes warnings
```

This catches issues **before** your app starts handling requests — no more runtime DI exceptions.

---

## 📦 Examples

### Simple Scoped Service

```csharp
[RegisterAsScoped]
public class ProductService : IProductService
{
    public Task<Product> CreateProduct(Product product)
    {
        // Implementation
    }
}
```

### Singleton with TryAdd

```csharp
[RegisterAsSingleton(useTryAddSingleton: true)]
public class CurrentUserProvider
{
    public Task<User> GetCurrentUser()
    {
        // Implementation
    }
}
```

---

## 🔑 Keyed Services (.NET 8+)

Register and resolve services by key:

```csharp
[RegisterAsSingletonKeyed("primary")]
public class PrimaryEmailService : IEmailService { }

[RegisterAsSingletonKeyed("secondary")]
public class SecondaryEmailService : IEmailService { }
```

```csharp
public class EmailManager
{
    public EmailManager(
        [FromKeyedServices("primary")] IEmailService primary,
        [FromKeyedServices("secondary")] IEmailService secondary)
    {
        // Use services
    }
}
```

---

## 🧱 Decorator Pattern Support

Easily layer cross-cutting concerns:

```csharp
public interface INotificationService
{
    MessageDto Send(string message);
}

[RegisterAsScoped]
[DecorateWith(typeof(LoggingDecorator), order: 0)]
[DecorateWith(typeof(StoreNotificationDecorator), order: 1)]
public class EmailNotificationService : INotificationService
{
    public MessageDto Send(string message)
    {
        // Send email notification
    }
}

public class LoggingDecorator : INotificationService
{
    private readonly INotificationService _inner;
    private readonly ILogger _logger;

    public LoggingDecorator(INotificationService inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public MessageDto Send(string message)
    {
        _logger.LogInformation("Sending: {message}", message);
        _inner.Send(message);
        _logger.LogInformation("Sent: {message}", message);
    }
}

public class StoreNotificationDecorator : INotificationService
{
    private readonly INotificationService _inner;
    private readonly DbContext _dbContext;

    public StoreNotificationDecorator(INotificationService inner, DbContext dbContext)
    {
        _inner = inner;
        _dbContext = dbContext;
    }

    public void Send(string message)
    {
       var messageDto = _inner.Send(message);
       _dbContext.Messages.Add(messageDto);
       _dbContext.SaveChanges();
    }
}
```

---

## 🛡️ Validation

EasyServiceRegister includes a built-in validation system that detects common DI misconfigurations at startup — before they become runtime exceptions.

### Quick Setup

```csharp
builder.Services.AddServices(typeof(Program));
builder.Services.EnsureServicesAreValid(); // Throws ServiceValidationException on errors
```

`EnsureServicesAreValid()` throws a `ServiceValidationException` if any **errors** are found. Warnings are included in the exception details but don't trigger a throw on their own. The method returns the `IServiceCollection` for chaining.

### What It Detects

| Issue | Severity | Description |
|-------|----------|-------------|
| **Missing dependencies** | Error | A service's constructor requires a type that isn't registered |
| **Scoped in singleton** | Error | A singleton depends on a scoped service (fails at runtime) |
| **Captive dependency chain** | Error | A singleton -> singleton -> scoped chain captures a scoped service |
| **Circular dependencies** | Error | A -> B -> C -> A cycles in the dependency graph |
| **Disposable transients** | Warning | A transient implements `IDisposable` but won't be disposed by the container |
| **Transient in singleton** | Warning | A singleton depends on a transient (instantiated only once) |

### Advanced Usage

For more control, use `ValidateServices()` directly:

```csharp
var issues = builder.Services.ValidateServices();

foreach (var issue in issues)
{
    Console.WriteLine($"[{issue.Severity}] {issue.Message}");
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
        // issue.ServiceType and issue.ImplementationType available for programmatic handling
    }
}
```

---

## 🔍 Diagnostics

### List Registered Services

```csharp
var registered = ServicesExtension.GetRegisteredServices();

foreach (var svc in registered)
{
    Console.WriteLine($"Type: {svc.ServiceType}, Impl: {svc.ImplementationType}, Lifetime: {svc.Lifetime}, Key: {svc.ServiceKey}");
}
```

Filter by type or lifetime:

```csharp
var singletons = ServicesExtension.GetRegisteredServices(lifetime: ServiceLifetime.Singleton);
var specific = ServicesExtension.GetRegisteredServices(serviceType: typeof(IMyService));
```

---

## ✅ Why Use EasyServiceRegister?

- ✅ Eliminates repetitive service registration
- ✅ Works with standard `IServiceCollection`
- ✅ Supports decorators, keyed services, and diagnostics
- ✅ Keeps your startup file clean and maintainable
- ✅ Catches DI misconfigurations at startup, not at runtime

---

## 📄 License

MIT ©
