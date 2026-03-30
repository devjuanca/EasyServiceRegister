using EasyServiceRegister.Exceptions;
using EasyServiceRegister.Tests.Fixtures;
using EasyServiceRegister.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.Tests;

public class ValidationTests : IDisposable
{
    public ValidationTests()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    public void Dispose()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    [Fact]
    public void Validation_DetectsMissingDependency()
    {
        var services = new ServiceCollection();
        services.AddServices(t => t == typeof(ServiceWithMissingDep), typeof(ServiceWithMissingDep));

        var issues = services.ValidateServices().ToList();

        Assert.Contains(issues, i =>
            i.Severity == ValidationSeverity.Error &&
            i.Message.Contains("IMissingDep") &&
            i.Message.Contains("not registered"));
    }

    [Fact]
    public void Validation_DetectsScopedInSingleton()
    {
        var services = new ServiceCollection();
        services.AddServices(
            t => t == typeof(SingletonWithScopedDep) || t == typeof(ScopedDependency),
            typeof(SingletonWithScopedDep));

        var issues = services.ValidateServices().ToList();

        Assert.Contains(issues, i =>
            i.Severity == ValidationSeverity.Error &&
            i.Message.Contains("scoped service") &&
            i.ImplementationType == typeof(SingletonWithScopedDep));
    }

    [Fact]
    public void Validation_WarnsTransientInSingleton()
    {
        var services = new ServiceCollection();
        services.AddServices(
            t => t == typeof(SingletonWithTransientDep) || t == typeof(TransientDependency),
            typeof(SingletonWithTransientDep));

        var issues = services.ValidateServices().ToList();

        Assert.Contains(issues, i =>
            i.Severity == ValidationSeverity.Warning &&
            i.Message.Contains("transient service") &&
            i.ImplementationType == typeof(SingletonWithTransientDep));
    }

    [Fact]
    public void Validation_DetectsCircularDependencies()
    {
        var services = new ServiceCollection();
        services.AddServices(
            t => t == typeof(CircularA) || t == typeof(CircularB) || t == typeof(CircularC),
            typeof(CircularA));

        var issues = services.ValidateServices().ToList();

        Assert.Contains(issues, i =>
            i.Severity == ValidationSeverity.Error &&
            i.Message.Contains("Dependency cycle detected"));
    }

    [Fact]
    public void Validation_MinimumSeverityFilters()
    {
        var services = new ServiceCollection();
        services.AddServices(
            t => t == typeof(SingletonWithTransientDep) || t == typeof(TransientDependency),
            typeof(SingletonWithTransientDep));

        var errorsOnly = services.ValidateServices(ValidationSeverity.Error).ToList();
        var allIssues = services.ValidateServices(ValidationSeverity.Warning).ToList();

        Assert.True(allIssues.Count >= errorsOnly.Count);
        Assert.All(errorsOnly, i => Assert.Equal(ValidationSeverity.Error, i.Severity));
    }

    [Fact]
    public void ValidationIssue_ToStringIncludesSeverity()
    {
        var issue = new ValidationIssue("Test message", ValidationSeverity.Error);

        Assert.Contains("[Error]", issue.ToString());
        Assert.Contains("Test message", issue.ToString());
    }

    // === New validation rules ===

    [Fact]
    public void Validation_WarnsDisposableTransient()
    {
        var services = new ServiceCollection();
        services.AddServices(
            t => t == typeof(DisposableTransientService),
            typeof(DisposableTransientService));

        var issues = services.ValidateServices(ValidationSeverity.Warning).ToList();

        Assert.Contains(issues, i =>
            i.Severity == ValidationSeverity.Warning &&
            i.Message.Contains("IDisposable") &&
            i.Message.Contains("memory leak") &&
            i.ImplementationType == typeof(DisposableTransientService));
    }

    [Fact]
    public void Validation_DetectsCaptiveDependencyChain()
    {
        var services = new ServiceCollection();
        services.AddServices(
            t => t == typeof(OuterSingleton) || t == typeof(MiddleSingleton) || t == typeof(ScopedDependency),
            typeof(OuterSingleton));

        var issues = services.ValidateServices().ToList();

        Assert.Contains(issues, i =>
            i.Severity == ValidationSeverity.Error &&
            i.Message.Contains("Captive dependency") &&
            i.Message.Contains("OuterSingleton") &&
            i.Message.Contains("MiddleSingleton") &&
            i.Message.Contains("IScopedDependency"));
    }

    // === EnsureServicesAreValid ===

    [Fact]
    public void EnsureServicesAreValid_ThrowsOnErrors()
    {
        var services = new ServiceCollection();
        services.AddServices(
            t => t == typeof(SingletonWithScopedDep) || t == typeof(ScopedDependency),
            typeof(SingletonWithScopedDep));

        var ex = Assert.Throws<ServiceValidationException>(() =>
            services.EnsureServicesAreValid());

        Assert.NotEmpty(ex.Issues);
        Assert.Contains(ex.Issues, i => i.Severity == ValidationSeverity.Error);
        Assert.Contains("scoped service", ex.Message);
    }

    [Fact]
    public void EnsureServicesAreValid_DoesNotThrowOnWarningsOnly()
    {
        var services = new ServiceCollection();
        services.AddServices(
            t => t == typeof(DisposableTransientService),
            typeof(DisposableTransientService));

        // Should not throw — disposable transient is a warning, not an error
        var result = services.EnsureServicesAreValid();

        Assert.Same(services, result);
    }

    [Fact]
    public void EnsureServicesAreValid_ReturnsSameCollection_WhenClean()
    {
        var services = new ServiceCollection();
        services.AddServices(t => t == typeof(CleanService), typeof(CleanService));

        var result = services.EnsureServicesAreValid();

        Assert.Same(services, result);
    }

    [Fact]
    public void EnsureServicesAreValid_ExceptionContainsAllIssues()
    {
        var services = new ServiceCollection();
        // Register a singleton with scoped dep (error) + a disposable transient (warning)
        services.AddServices(
            t => t == typeof(SingletonWithScopedDep) || t == typeof(ScopedDependency) || t == typeof(DisposableTransientService),
            typeof(SingletonWithScopedDep));

        var ex = Assert.Throws<ServiceValidationException>(() =>
            services.EnsureServicesAreValid());

        // Should contain the error
        Assert.Contains(ex.Issues, i => i.Severity == ValidationSeverity.Error);
        // Should also include warnings
        Assert.Contains(ex.Issues, i => i.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void EnsureServicesAreValid_ExceptionMessageIsFormatted()
    {
        var services = new ServiceCollection();
        services.AddServices(
            t => t == typeof(ServiceWithMissingDep),
            typeof(ServiceWithMissingDep));

        var ex = Assert.Throws<ServiceValidationException>(() =>
            services.EnsureServicesAreValid());

        Assert.Contains("Service validation failed", ex.Message);
        Assert.Contains("[Error]", ex.Message);
    }

    [Fact]
    public void ServiceValidationException_IssuesAreAccessible()
    {
        var issues = new List<ValidationIssue>
        {
            new ValidationIssue("Test error", ValidationSeverity.Error),
            new ValidationIssue("Test warning", ValidationSeverity.Warning),
        };

        var ex = new ServiceValidationException(issues);

        Assert.Equal(2, ex.Issues.Count);
        Assert.Contains("1 error(s) and 1 warning(s)", ex.Message);
    }
}
