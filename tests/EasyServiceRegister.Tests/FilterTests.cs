using EasyServiceRegister.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.Tests;

public class FilterTests : IDisposable
{
    private readonly ServiceCollection _services;

    public FilterTests()
    {
        ServicesExtension.ClearRegistrationLog();
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    [Fact]
    public void Filter_IncludesMatchingTypes()
    {
        _services.AddServices(
            filter: t => t == typeof(IncludedFilteredService),
            typeof(IncludedFilteredService));
        var provider = _services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IFilteredService>();
        Assert.IsType<IncludedFilteredService>(service);
    }

    [Fact]
    public void Filter_ExcludedTypeNotRegistered()
    {
        _services.AddServices(
            filter: t => t == typeof(IncludedFilteredService),
            typeof(IncludedFilteredService));

        var registrations = ServicesExtension.GetRegisteredServices();

        Assert.DoesNotContain(registrations, r => r.ImplementationType == typeof(ExcludedFilteredService));
        Assert.Contains(registrations, r => r.ImplementationType == typeof(IncludedFilteredService));
    }

    [Fact]
    public void Filter_WithAssemblyOverload_Works()
    {
        var assembly = typeof(IncludedFilteredService).Assembly;
        _services.AddServices(
            filter: t => t == typeof(IncludedFilteredService),
            assembly);

        var registrations = ServicesExtension.GetRegisteredServices();

        Assert.DoesNotContain(registrations, r => r.ImplementationType == typeof(ExcludedFilteredService));
        Assert.Single(registrations);
    }

    [Fact]
    public void Filter_ExcludeAll_RegistersNothing()
    {
        _services.AddServices(
            filter: _ => false,
            typeof(IncludedFilteredService));

        var registrations = ServicesExtension.GetRegisteredServices();

        Assert.Empty(registrations);
    }
}
