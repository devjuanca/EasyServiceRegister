# Advanced Features Guide

This guide covers the advanced features added to EasyServiceRegister to enhance functionality and provide more flexibility.

## Table of Contents

1. [Convention-Based Registration](#convention-based-registration)
2. [Batch Registration](#batch-registration)
3. [Assembly Filtering](#assembly-filtering)
4. [Conditional Registration](#conditional-registration)
5. [Registration Interceptors](#registration-interceptors)
6. [Enhanced Diagnostics](#enhanced-diagnostics)

---

## Convention-Based Registration

Automatically register services based on naming conventions without using attributes.

**Important:** By default, convention-based registration:
- **Skips types with attributes** - Services already marked with `[RegisterAsScoped]`, `[RegisterAsSingleton]`, etc. are skipped to avoid duplicates
- **Uses TryAdd** - Won't override existing registrations, preventing conflicts when combined with attribute-based registration

You can control this behavior with the `skipTypesWithAttributes` and `useTryAdd` parameters.

### Register by Naming Convention

```csharp
using EasyServiceRegister.Conventions;

// Registers IUserService -> UserService, IProductService -> ProductService, etc.
services.AddServicesByConvention(
    typeof(Program).Assembly,
    lifetime: ServiceLifetime.Scoped,
    interfaceNamePattern: "I{0}");

// Override defaults to include services with attributes and use Add instead of TryAdd
services.AddServicesByConvention(
    typeof(Program).Assembly,
    lifetime: ServiceLifetime.Scoped,
    interfaceNamePattern: "I{0}",
    skipTypesWithAttributes: false,
    useTryAdd: false);
```

### Register by Suffix

```csharp
// Registers all classes ending with "Service"
services.AddServicesBySuffix(
    typeof(Program).Assembly,
    suffix: "Service",
    lifetime: ServiceLifetime.Scoped);

// Registers all classes ending with "Repository"
services.AddServicesBySuffix(
    typeof(Program).Assembly,
    suffix: "Repository",
    lifetime: ServiceLifetime.Scoped);
```

### Register by Namespace

```csharp
// Register all services in a specific namespace
services.AddServicesByNamespace(
    typeof(Program).Assembly,
    namespace: "MyApp.Services",
    lifetime: ServiceLifetime.Scoped,
    includeSubNamespaces: true);
```

### Combining with Attribute-Based Registration

The recommended approach is to call `AddServices()` first, then use convention-based registration:

```csharp
// 1. Register services with attributes first (these have priority)
services.AddServices(typeof(Program));

// 2. Convention-based registration will skip types with attributes and use TryAdd
services.AddServicesByConvention(
    typeof(Program).Assembly,
    ServiceLifetime.Scoped);
```

This ensures:
- Services with explicit attributes are registered as configured
- Convention-based registration fills in the gaps for services without attributes
- No duplicate registrations occur

If you want convention-based to take priority, reverse the order:

```csharp
// 1. Register by convention first
services.AddServicesByConvention(
    typeof(Program).Assembly,
    ServiceLifetime.Scoped);

// 2. Attribute-based registration (use TryAdd in attributes to avoid overriding)
services.AddServices(typeof(Program));
```

---

## Batch Registration

Register multiple services at once with a fluent API.

### Using AddMultiple

```csharp
using EasyServiceRegister.Batch;

// Register multiple implementations of the same interface
services.AddMultiple<INotificationService>(
    ServiceLifetime.Scoped,
    typeof(EmailNotificationService),
    typeof(SmsNotificationService),
    typeof(PushNotificationService));
```

### Using Fluent Builder

```csharp
services
    .WithLifetime(ServiceLifetime.Scoped)
    .Add<IEmailService, SmtpEmailService>()
    .Add<ISmsService, TwilioSmsService>()
    .Add<IPaymentService, StripePaymentService>()
    .Build();
```

---

## Assembly Filtering

Filter types during assembly scanning for more precise control.

### Basic Filtering

```csharp
using EasyServiceRegister.Filtering;

services.AddServicesWithFilter(
    typeof(Program).Assembly,
    options =>
    {
        // Include only services in specific namespaces
        options.IncludeNamespaces.Add("MyApp.Services");
        options.IncludeNamespaces.Add("MyApp.Repositories");
        
        // Exclude test services
        options.ExcludeNamespaces.Add("MyApp.Services.Test");
        
        // Include only public types
        options.PublicTypesOnly = true;
        
        // Exclude abstract classes
        options.IncludeAbstractClasses = false;
    });
```

### Pattern-Based Filtering

```csharp
services.AddServicesWithFilter(
    typeof(Program).Assembly,
    options =>
    {
        // Include types matching patterns
        options.IncludeTypePatterns.Add("*Service");
        options.IncludeTypePatterns.Add("*Repository");
        
        // Exclude types matching patterns
        options.ExcludeTypePatterns.Add("*Mock*");
        options.ExcludeTypePatterns.Add("*Test*");
    });
```

---

## Conditional Registration

Register services conditionally based on environment or custom logic.

### Environment-Based Registration

```csharp
using EasyServiceRegister.Attributes;

// Register only in Development
[RegisterAsScoped]
[RegisterWhen("Development")]
public class DevelopmentEmailService : IEmailService
{
    // Implementation for development
}

// Register only in Production
[RegisterAsScoped]
[RegisterWhen("Production")]
public class ProductionEmailService : IEmailService
{
    // Implementation for production
}

// Register in multiple environments
[RegisterAsScoped]
[RegisterWhen(new[] { "Staging", "Production" })]
public class CloudStorageService : IStorageService
{
    // Implementation
}
```

### Exclude from Environments

```csharp
// Register in all environments except Test
[RegisterAsScoped]
[RegisterWhen("Test", includeEnvironment: false)]
public class RealDatabaseService : IDatabaseService
{
    // Implementation
}
```

---

## Registration Interceptors

Intercept and customize the registration process.

### Logging Interceptor

```csharp
using EasyServiceRegister.Interceptors;

var loggingInterceptor = new LoggingRegistrationInterceptor(
    message => Console.WriteLine($"[Registration] {message}"));

// Use with registration process
// (Note: Integration with AddServices would need to be added)
```

### Validation Interceptor

```csharp
var validationInterceptor = new ValidationRegistrationInterceptor(
    context => 
    {
        // Only allow services from approved namespaces
        return context.ImplementationType?.Namespace?.StartsWith("MyApp") == true;
    });
```

### Custom Interceptor

```csharp
public class CustomInterceptor : RegistrationInterceptorBase
{
    public override void BeforeRegistration(RegistrationContext context)
    {
        // Custom logic before registration
        if (context.ServiceType.Name.Contains("Legacy"))
        {
            context.Skip = true;
        }
    }

    public override void AfterRegistration(RegistrationContext context)
    {
        // Custom logic after registration
        Console.WriteLine($"Registered: {context.ServiceType.Name}");
    }
}
```

---

## Enhanced Diagnostics

Advanced diagnostic capabilities for analyzing service registrations.

### Generate Detailed Report

```csharp
using EasyServiceRegister.Diagnostics;

// After calling AddServices
var report = EnhancedDiagnostics.GenerateRegistrationReport();
Console.WriteLine(report);

// Output:
// === EasyServiceRegister Diagnostic Report ===
// Total Services Registered: 42
//
// --- Singleton Services (5) ---
//   Service: MyApp.Services.IConfigurationService
//   Implementation: MyApp.Services.ConfigurationService
//   ...
```

### Get Registration Statistics

```csharp
var stats = EnhancedDiagnostics.GetRegistrationStatistics();

Console.WriteLine($"Total Services: {stats["TotalServices"]}");
Console.WriteLine($"Singletons: {stats["SingletonCount"]}");
Console.WriteLine($"Scoped: {stats["ScopedCount"]}");
Console.WriteLine($"Transient: {stats["TransientCount"]}");
Console.WriteLine($"Keyed Services: {stats["KeyedServicesCount"]}");
Console.WriteLine($"Decorated Services: {stats["DecoratedServicesCount"]}");
```

### Find Services

```csharp
// Find services by name pattern
var userServices = EnhancedDiagnostics.FindServicesByName("User");

// Find all services implementing an interface
var repositories = EnhancedDiagnostics.GetServicesByBaseType(typeof(IRepository));
```

### Visualize Dependencies

```csharp
var graph = EnhancedDiagnostics.GenerateDependencyGraph();
Console.WriteLine(graph);

// Output:
// === Service Dependency Graph ===
// IUserService
//   └─ UserService [Scoped]
//      └─ Decorator: LoggingDecorator
//      └─ Decorator: CachingDecorator
```

### Export to CSV

```csharp
var csv = EnhancedDiagnostics.ExportToCsv();
File.WriteAllText("service-registrations.csv", csv);
```

---

## Best Practices

### Combining Features

You can combine multiple features for powerful registration scenarios. Convention-based registration is designed to work seamlessly with attribute-based registration:

```csharp
// 1. Use attributes for core services (these have priority)
services.AddServices(typeof(Program));

// 2. Use conventions for repositories (automatically skips types with attributes)
services.AddServicesByConvention(
    typeof(Program).Assembly,
    lifetime: ServiceLifetime.Scoped,
    interfaceNamePattern: "I{0}Repository");

// 3. Use batch registration for specific groups
services
    .WithLifetime(ServiceLifetime.Singleton)
    .Add<ICache, RedisCache>()
    .Add<ILogger, FileLogger>()
    .Build();

// 4. Validate registrations
var issues = services.ValidateServices();
foreach (var issue in issues)
{
    Console.WriteLine(issue);
}

// 5. Generate diagnostic report
var report = EnhancedDiagnostics.GenerateRegistrationReport();
File.WriteAllText("diagnostics.txt", report);
```

**Note:** Convention-based methods automatically skip types with EasyServiceRegister attributes and use `TryAdd` by default, preventing duplicate registrations. This allows you to safely combine different registration approaches.

### Performance Considerations

- Convention-based registration scans all types, so use filtering to reduce overhead
- Use batch registration for multiple services with the same lifetime
- Cache assembly scanning results when possible
- Use diagnostics in development/testing, not production

### Testing

```csharp
// In test projects, use conditional registration
[RegisterAsScoped]
[RegisterWhen("Test")]
public class MockEmailService : IEmailService
{
    // Test implementation
}

// Or use filtering to exclude production services
services.AddServicesWithFilter(
    typeof(Program).Assembly,
    options =>
    {
        options.ExcludeNamespaces.Add("MyApp.Production");
    });
```

---

## Migration Guide

### From Standard Registration

**Before:**
```csharp
services.AddScoped<IUserService, UserService>();
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IOrderService, OrderService>();
```

**After:**
```csharp
// Option 1: Use attributes
[RegisterAsScoped]
public class UserService : IUserService { }

services.AddServices(typeof(Program));

// Option 2: Use conventions
services.AddServicesBySuffix(
    typeof(Program).Assembly,
    "Service",
    ServiceLifetime.Scoped);

// Option 3: Use batch registration
services
    .WithLifetime(ServiceLifetime.Scoped)
    .Add<IUserService, UserService>()
    .Add<IProductService, ProductService>()
    .Add<IOrderService, OrderService>()
    .Build();
```

---

## Troubleshooting

### Services Not Being Registered

1. Ensure the assembly is being scanned
2. Check namespace filters
3. Verify attribute spelling and parameters
4. Use diagnostics to see what was registered:
   ```csharp
   var report = EnhancedDiagnostics.GenerateRegistrationReport();
   Console.WriteLine(report);
   ```

### Performance Issues

1. Use assembly filtering to reduce scanning
2. Cache assembly references
3. Use batch registration for multiple similar services
4. Profile with diagnostics statistics

---

## Examples Repository

For complete working examples, see the `sample` directory in the repository.
