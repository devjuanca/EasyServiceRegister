using EasyServiceRegister.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.Tests;

public class DiagnosticsTests : IDisposable
{
    private readonly ServiceCollection _services;

    public DiagnosticsTests()
    {
        ServicesExtension.ClearRegistrationLog();
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    [Fact]
    public void GetRegisteredServices_ReturnsAllRegistrations()
    {
        _services.AddServices(
            t => t == typeof(SingletonService) || t == typeof(ScopedService),
            typeof(SingletonService));

        var registrations = ServicesExtension.GetRegisteredServices().ToList();

        Assert.Equal(2, registrations.Count);
    }

    [Fact]
    public void GetRegisteredServices_FilterByType()
    {
        _services.AddServices(
            t => t == typeof(SingletonService) || t == typeof(ScopedService),
            typeof(SingletonService));

        var registrations = ServicesExtension.GetRegisteredServices(
            serviceType: typeof(ISingletonService)).ToList();

        Assert.Single(registrations);
        Assert.Equal(typeof(ISingletonService), registrations[0].ServiceType);
        Assert.Equal(typeof(SingletonService), registrations[0].ImplementationType);
    }

    [Fact]
    public void GetRegisteredServices_FilterByLifetime()
    {
        _services.AddServices(
            t => t == typeof(SingletonService) || t == typeof(ScopedService),
            typeof(SingletonService));

        var singletons = ServicesExtension.GetRegisteredServices(
            lifetime: ServiceLifetime.Singleton).ToList();

        Assert.Single(singletons);
        Assert.All(singletons, r => Assert.Equal(ServiceLifetime.Singleton, r.Lifetime));
    }

    [Fact]
    public void GetRegisteredServices_ContainsCorrectAttributeName()
    {
        _services.AddServices(t => t == typeof(SingletonService), typeof(SingletonService));

        var reg = ServicesExtension.GetRegisteredServices(
            serviceType: typeof(ISingletonService)).First();

        Assert.Equal("RegisterAsSingleton", reg.AttributeUsed);
    }

    [Fact]
    public void ClearRegistrationLog_EmptiesTheLog()
    {
        _services.AddServices(t => t == typeof(SingletonService), typeof(SingletonService));
        Assert.NotEmpty(ServicesExtension.GetRegisteredServices());

        ServicesExtension.ClearRegistrationLog();

        Assert.Empty(ServicesExtension.GetRegisteredServices());
    }

    [Fact]
    public void KeyedService_LogsServiceKey()
    {
        _services.AddServices(t => t == typeof(PrimaryKeyedService), typeof(PrimaryKeyedService));

        var registrations = ServicesExtension.GetRegisteredServices().ToList();
        var keyed = registrations.Where(r => r.ServiceKey != null).ToList();

        Assert.NotEmpty(keyed);
        Assert.Contains(keyed, r => (string)r.ServiceKey == "primary");
    }

    [Fact]
    public void RegistrationLog_TracksRegistrationMethod()
    {
        _services.AddServices(t => t == typeof(SingletonService), typeof(SingletonService));

        var registrations = ServicesExtension.GetRegisteredServices().ToList();

        Assert.All(registrations, r =>
            Assert.True(r.RegistrationMethod == "Add" || r.RegistrationMethod == "TryAdd"));
    }

    [Fact]
    public void KeyedService_LogsCorrectAttributeName()
    {
        _services.AddServices(t => t == typeof(PrimaryKeyedService), typeof(PrimaryKeyedService));

        var reg = ServicesExtension.GetRegisteredServices().First();

        Assert.Equal("RegisterAsScopedKeyed", reg.AttributeUsed);
    }
}
