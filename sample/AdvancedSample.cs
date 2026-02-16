using EasyServiceRegister;
using EasyServiceRegister.Batch;
using EasyServiceRegister.Conventions;
using EasyServiceRegister.Diagnostics;
using EasyServiceRegister.Filtering;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.AdvancedSample
{
    /// <summary>
    /// Sample demonstrating advanced features of EasyServiceRegister
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();

            // 1. Convention-based registration
            Console.WriteLine("=== Convention-Based Registration ===");
            services.AddServicesByConvention(
                typeof(Program).Assembly,
                ServiceLifetime.Scoped,
                interfaceNamePattern: "I{0}");

            // 2. Register by suffix
            services.AddServicesBySuffix(
                typeof(Program).Assembly,
                "Repository",
                ServiceLifetime.Scoped);

            // 3. Batch registration with fluent API
            Console.WriteLine("\n=== Batch Registration ===");
            services
                .WithLifetime(ServiceLifetime.Singleton)
                .Add<ICacheService, RedisCacheService>()
                .Add<ILoggerService, FileLoggerService>()
                .Build();

            // 4. Assembly filtering
            Console.WriteLine("\n=== Assembly Filtering ===");
            services.AddServicesWithFilter(
                typeof(Program).Assembly,
                options =>
                {
                    options.IncludeNamespaces.Add("EasyServiceRegister.AdvancedSample.Services");
                    options.ExcludeTypePatterns.Add("*Test*");
                    options.PublicTypesOnly = true;
                });

            // 5. Standard attribute-based registration
            Console.WriteLine("\n=== Attribute-Based Registration ===");
            services.AddServices(typeof(Program));

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // 6. Enhanced Diagnostics
            Console.WriteLine("\n=== Enhanced Diagnostics ===\n");

            // Get statistics
            var stats = EnhancedDiagnostics.GetRegistrationStatistics();
            Console.WriteLine($"Total Services: {stats["TotalServices"]}");
            Console.WriteLine($"Singleton: {stats["SingletonCount"]}");
            Console.WriteLine($"Scoped: {stats["ScopedCount"]}");
            Console.WriteLine($"Transient: {stats["TransientCount"]}");
            Console.WriteLine($"Keyed Services: {stats["KeyedServicesCount"]}");
            Console.WriteLine($"Decorated Services: {stats["DecoratedServicesCount"]}");

            // Generate detailed report
            Console.WriteLine("\n=== Detailed Report ===\n");
            var report = EnhancedDiagnostics.GenerateRegistrationReport();
            Console.WriteLine(report);

            // Generate dependency graph
            Console.WriteLine("\n=== Dependency Graph ===\n");
            var graph = EnhancedDiagnostics.GenerateDependencyGraph();
            Console.WriteLine(graph);

            // Find services by name
            Console.WriteLine("\n=== Find Services ===\n");
            var userServices = EnhancedDiagnostics.FindServicesByName("User");
            foreach (var svc in userServices)
            {
                Console.WriteLine($"Found: {svc.ServiceType.Name} -> {svc.ImplementationType.Name}");
            }

            // Export to CSV
            Console.WriteLine("\n=== Exporting to CSV ===");
            var csv = EnhancedDiagnostics.ExportToCsv();
            File.WriteAllText("service-registrations.csv", csv);
            Console.WriteLine("Exported to service-registrations.csv");

            Console.WriteLine("\n=== Sample Completed ===");
        }
    }

    // Sample service interfaces and implementations

    public interface ICacheService
    {
        void Set(string key, object value);
        object Get(string key);
    }

    public class RedisCacheService : ICacheService
    {
        public void Set(string key, object value) => Console.WriteLine($"Redis: Set {key}");
        public object Get(string key) => null;
    }

    public interface ILoggerService
    {
        void Log(string message);
    }

    public class FileLoggerService : ILoggerService
    {
        public void Log(string message) => Console.WriteLine($"File: {message}");
    }

    // Convention-based services (IUserService -> UserService)
    public interface IUserService
    {
        string GetUser(int id);
    }

    public class UserService : IUserService
    {
        public string GetUser(int id) => $"User {id}";
    }

    public interface IProductService
    {
        string GetProduct(int id);
    }

    public class ProductService : IProductService
    {
        public string GetProduct(int id) => $"Product {id}";
    }

    // Repository pattern
    public interface IUserRepository
    {
        void Save(object user);
    }

    public class UserRepository : IUserRepository
    {
        public void Save(object user) => Console.WriteLine("Saved user");
    }

    public interface IProductRepository
    {
        void Save(object product);
    }

    public class ProductRepository : IProductRepository
    {
        public void Save(object product) => Console.WriteLine("Saved product");
    }
}
