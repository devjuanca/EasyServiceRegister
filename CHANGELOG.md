# Changelog

All notable changes to EasyServiceRegister will be documented in this file.

## [Unreleased] - 2026-02-16

### Added - Advanced Features

#### Convention-Based Registration
- **AddServicesByConvention**: Automatically register services based on naming conventions (e.g., IUserService -> UserService)
- **AddServicesBySuffix**: Register all classes with a specific suffix (e.g., "Service", "Repository")
- **AddServicesByNamespace**: Register all services within a specific namespace

#### Batch Registration
- **AddMultiple**: Register multiple implementations of the same interface at once
- **Fluent API**: Chain multiple service registrations with `WithLifetime().Add().Add().Build()`
- Simplifies registration of groups of related services

#### Assembly Filtering
- **AssemblyFilterOptions**: Fine-grained control over type scanning
- **Namespace filtering**: Include/exclude specific namespaces
- **Pattern matching**: Filter types by name patterns with wildcard support
- **Type filtering**: Control inclusion of abstract classes, generic types, and visibility

#### Conditional Registration
- **RegisterWhenAttribute**: Register services conditionally based on environment
- Support for multiple environments
- Include or exclude mode for environment filtering
- Perfect for development/staging/production specific implementations

#### Registration Interceptors
- **IRegistrationInterceptor**: Hook into the registration process
- **LoggingRegistrationInterceptor**: Built-in logging of all registrations
- **ValidationRegistrationInterceptor**: Validate registrations before they occur
- **Custom interceptors**: Create your own interceptor logic with BeforeRegistration and AfterRegistration hooks

#### Enhanced Diagnostics
- **GenerateRegistrationReport**: Detailed text report of all registrations grouped by lifetime
- **GetRegistrationStatistics**: Dictionary of statistics (counts by lifetime, decorated services, etc.)
- **FindServicesByName**: Search for services by name pattern
- **GetServicesByBaseType**: Find all services implementing a specific interface
- **GenerateDependencyGraph**: Visual text representation of service dependencies
- **ExportToCsv**: Export registration information to CSV format

### Documentation
- **ADVANCED_FEATURES.md**: Comprehensive guide covering all new features with examples
- **README.md**: Updated with links to advanced features documentation
- **AdvancedSample.cs**: Working sample demonstrating all new features

### Benefits
- **Reduced Boilerplate**: Convention-based and batch registration eliminate repetitive code
- **Better Organization**: Filter and organize services by namespace, pattern, or environment
- **Enhanced Visibility**: Comprehensive diagnostics provide insights into service registrations
- **Greater Flexibility**: Interceptors and conditional registration provide fine-grained control
- **Improved Debugging**: Detailed reports and statistics help troubleshoot registration issues

### Technical Improvements
- All new features maintain backward compatibility
- Zero breaking changes to existing API
- Target frameworks: netstandard2.1 and net8.0
- Full XML documentation for IntelliSense support
- Performance-conscious implementations with LINQ optimizations

### Use Cases

**Convention-Based Registration** is ideal for:
- Large projects with consistent naming patterns
- Reducing registration boilerplate
- Auto-discovery of services

**Batch Registration** is ideal for:
- Registering multiple related services
- Cleaner, more readable registration code
- Fluent configuration style

**Assembly Filtering** is ideal for:
- Large assemblies where you only need specific services
- Excluding test or internal types
- Performance optimization by reducing scanning

**Conditional Registration** is ideal for:
- Different implementations per environment
- Development vs. production services
- Feature flags and A/B testing scenarios

**Registration Interceptors** are ideal for:
- Logging and auditing registrations
- Validation and security checks
- Custom registration logic

**Enhanced Diagnostics** are ideal for:
- Troubleshooting registration issues
- Documentation generation
- Understanding service dependencies
- Performance analysis

---

## [3.0.5] - Previous Release

### Features
- Attribute-based service registration
- Support for Singleton, Scoped, and Transient lifetimes
- Keyed services support (.NET 8+)
- Decorator pattern implementation
- Basic diagnostics and validation
- Open generic type support

---

## Future Considerations

Potential future enhancements being considered:
- Integration with IConfiguration for configuration-based registration
- Support for factory methods in attributes
- Async interceptor support
- Performance profiling tools
- Visual Studio extension for diagnostics visualization
- Source generator for compile-time registration
- Integration with popular DI containers beyond Microsoft.Extensions.DependencyInjection

---

## Notes

This project follows [Semantic Versioning](https://semver.org/).

For upgrade guides and migration information, see [ADVANCED_FEATURES.md](ADVANCED_FEATURES.md#migration-guide).
