# EasyServiceRegister

**Simple, Attribute-Based Dependency Injection for .NET**

EasyServiceRegister is a lightweight library built on top of `Microsoft.Extensions.DependencyInjection.Abstractions`. It simplifies the registration of services using attributes, supporting lifetimes, keyed services, decorators, and validation — all with minimal boilerplate.

> ✅ Now supporting **keyed services** (.NET 8+) and **registration diagnostics** in **v3.0.0+**

---

## 🚀 Installation

Install via NuGet:

```bash
dotnet add package EasyServiceRegister
```

---

## ⚙️ How It Works

### 1. Add a registration attribute to your service class:

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

---

### 2. Register All Services

Call `AddServices` in your `Startup.cs` or `Program.cs`:

```csharp
services.AddServices(typeof(MyServiceMarkerType)); // Marker class from your assembly

// or

services.AddServices(typeof(Assembly1), typeof(Assembly2));
```

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

## 🧪 Diagnostics & Validation

### 🔍 List Registered Services

```csharp
var registered = ServicesExtension.GetRegisteredServices();

foreach (var svc in registered)
{
    Console.WriteLine($"Type: {svc.ServiceType}, Impl: {svc.ImplementationType}, Lifetime: {svc.Lifetime}, Key: {svc.ServiceKey}");
}
```

### ⚠️ Detect Anti-Patterns

Find issues like circular dependencies or incorrect lifetimes:

```csharp
var issues = builder.Services.ValidateServices();

foreach (var issue in issues)
{
    Console.WriteLine(issue.Message);
}
```

---

## ✅ Why Use EasyServiceRegister?

- ✅ Eliminates repetitive service registration
- ✅ Works with standard `IServiceCollection`
- ✅ Supports decorators, keyed services, and diagnostics
- ✅ Keeps your startup file clean and maintainable

---

## 📄 License

MIT ©