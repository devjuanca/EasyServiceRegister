using EasyServiceRegister.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.Tests;

public class AssemblyRegistrationTests : IDisposable
{
    private readonly ServiceCollection _services;
    private static readonly HashSet<Type> CoreFixtures = new()
    {
        typeof(SingletonService),
        typeof(ScopedService),
        typeof(TransientService),
    };

    public AssemblyRegistrationTests()
    {
        ServicesExtension.ClearRegistrationLog();
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    [Fact]
    public void AddServices_ByMarkerType_RegistersServices()
    {
        _services.AddServices(t => CoreFixtures.Contains(t), typeof(SingletonService));

        var registrations = ServicesExtension.GetRegisteredServices().ToList();

        Assert.Equal(3, registrations.Count);
    }

    [Fact]
    public void AddServices_ByAssembly_RegistersServices()
    {
        _services.AddServices(t => CoreFixtures.Contains(t), typeof(SingletonService).Assembly);

        var registrations = ServicesExtension.GetRegisteredServices().ToList();

        Assert.Equal(3, registrations.Count);
    }

    [Fact]
    public void AddServices_ReturnsSameServiceCollection_ForChaining()
    {
        var result = _services.AddServices(typeof(SingletonService).Assembly);

        Assert.Same(_services, result);
    }

    [Fact]
    public void AddServices_FilterOverload_ByMarkerType_Works()
    {
        var result = _services.AddServices(
            filter: t => t == typeof(SingletonService),
            typeof(SingletonService));

        Assert.Same(_services, result);

        var registrations = ServicesExtension.GetRegisteredServices().ToList();
        Assert.Single(registrations);
        Assert.Equal(typeof(SingletonService), registrations[0].ImplementationType);
    }

    [Fact]
    public void AddServices_FilterOverload_ByAssembly_Works()
    {
        var result = _services.AddServices(
            filter: t => t == typeof(SingletonService),
            typeof(SingletonService).Assembly);

        Assert.Same(_services, result);

        var registrations = ServicesExtension.GetRegisteredServices().ToList();
        Assert.Single(registrations);
        Assert.Equal(typeof(SingletonService), registrations[0].ImplementationType);
    }

    [Fact]
    public void AddServices_FilterOverload_ReturnsSameServiceCollection()
    {
        var result = _services.AddServices(
            filter: _ => true,
            typeof(SingletonService).Assembly);

        Assert.Same(_services, result);
    }
}
