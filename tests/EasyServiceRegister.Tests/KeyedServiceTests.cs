using EasyServiceRegister.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.Tests;

public class KeyedServiceTests : IDisposable
{
    private readonly ServiceCollection _services;

    public KeyedServiceTests()
    {
        ServicesExtension.ClearRegistrationLog();
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    [Fact]
    public void StringKeyedScoped_ResolvesCorrectImplementation()
    {
        _services.AddServices(
            t => t == typeof(PrimaryKeyedService) || t == typeof(SecondaryKeyedService),
            typeof(PrimaryKeyedService));
        var provider = _services.BuildServiceProvider();

        var primary = provider.GetRequiredKeyedService<IKeyedService>("primary");
        var secondary = provider.GetRequiredKeyedService<IKeyedService>("secondary");

        Assert.IsType<PrimaryKeyedService>(primary);
        Assert.Equal("primary", primary.Key);
        Assert.IsType<SecondaryKeyedService>(secondary);
        Assert.Equal("secondary", secondary.Key);
    }

    [Fact]
    public void EnumKeyedSingleton_ResolvesCorrectImplementation()
    {
        _services.AddServices(
            t => t == typeof(EnumPrimaryKeyedService) || t == typeof(EnumSecondaryKeyedService),
            typeof(EnumPrimaryKeyedService));
        var provider = _services.BuildServiceProvider();

        var primary = provider.GetRequiredKeyedService<IKeyedService>(ServiceKeyEnum.Primary);
        var secondary = provider.GetRequiredKeyedService<IKeyedService>(ServiceKeyEnum.Secondary);

        Assert.IsType<EnumPrimaryKeyedService>(primary);
        Assert.Equal("EnumPrimary", primary.Key);
        Assert.IsType<EnumSecondaryKeyedService>(secondary);
        Assert.Equal("EnumSecondary", secondary.Key);
    }

    [Fact]
    public void KeyedTransient_ReturnsNewInstanceEachTime()
    {
        _services.AddServices(t => t == typeof(KeyedTransientService), typeof(KeyedTransientService));
        var provider = _services.BuildServiceProvider();

        var instance1 = provider.GetRequiredKeyedService<IKeyedService>("transient-key");
        var instance2 = provider.GetRequiredKeyedService<IKeyedService>("transient-key");

        Assert.NotSame(instance1, instance2);
    }

    [Fact]
    public void EnumKeyedSingleton_ReturnsSameInstance()
    {
        _services.AddServices(
            t => t == typeof(EnumPrimaryKeyedService) || t == typeof(EnumSecondaryKeyedService),
            typeof(EnumPrimaryKeyedService));
        var provider = _services.BuildServiceProvider();

        var instance1 = provider.GetRequiredKeyedService<IKeyedService>(ServiceKeyEnum.Primary);
        var instance2 = provider.GetRequiredKeyedService<IKeyedService>(ServiceKeyEnum.Primary);

        Assert.Same(instance1, instance2);
    }
}
