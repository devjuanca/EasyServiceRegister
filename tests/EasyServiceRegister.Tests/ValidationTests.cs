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
    public void Validation_DetectsDuplicateRegistrations()
    {
        var services = new ServiceCollection();
        services.AddServices(
            t => t == typeof(FirstDuplicateService) || t == typeof(SecondDuplicateService),
            typeof(FirstDuplicateService));

        var issues = services.ValidateServices(ValidationSeverity.Warning).ToList();

        var duplicateWarnings = issues.Where(i =>
            i.Severity == ValidationSeverity.Warning &&
            i.Message.Contains("Duplicate")).ToList();

        Assert.NotEmpty(duplicateWarnings);
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
}
