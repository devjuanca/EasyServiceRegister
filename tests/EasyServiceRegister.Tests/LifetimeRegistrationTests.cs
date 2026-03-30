using EasyServiceRegister.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.Tests;

public class LifetimeRegistrationTests : IDisposable
{
    private readonly ServiceCollection _services;

    public LifetimeRegistrationTests()
    {
        ServicesExtension.ClearRegistrationLog();
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    [Fact]
    public void Singleton_ReturnsSameInstance()
    {
        _services.AddServices(t => t == typeof(SingletonService), typeof(SingletonService));
        var provider = _services.BuildServiceProvider();

        var instance1 = provider.GetRequiredService<ISingletonService>();
        var instance2 = provider.GetRequiredService<ISingletonService>();

        Assert.Same(instance1, instance2);
        Assert.Equal(instance1.Id, instance2.Id);
    }

    [Fact]
    public void Scoped_ReturnsSameInstanceWithinScope_DifferentAcrossScopes()
    {
        _services.AddServices(t => t == typeof(ScopedService), typeof(ScopedService));
        var provider = _services.BuildServiceProvider();

        Guid scopeId1, scopeId2;
        using (var scope1 = provider.CreateScope())
        {
            var a = scope1.ServiceProvider.GetRequiredService<IScopedService>();
            var b = scope1.ServiceProvider.GetRequiredService<IScopedService>();
            Assert.Same(a, b);
            scopeId1 = a.Id;
        }

        using (var scope2 = provider.CreateScope())
        {
            var c = scope2.ServiceProvider.GetRequiredService<IScopedService>();
            scopeId2 = c.Id;
        }

        Assert.NotEqual(scopeId1, scopeId2);
    }

    [Fact]
    public void Transient_ReturnsNewInstanceEachTime()
    {
        _services.AddServices(t => t == typeof(TransientService), typeof(TransientService));
        var provider = _services.BuildServiceProvider();

        var instance1 = provider.GetRequiredService<ITransientService>();
        var instance2 = provider.GetRequiredService<ITransientService>();

        Assert.NotSame(instance1, instance2);
        Assert.NotEqual(instance1.Id, instance2.Id);
    }
}
