using EasyServiceRegister.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.Tests;

public class DecoratorTests : IDisposable
{
    private readonly ServiceCollection _services;

    public DecoratorTests()
    {
        ServicesExtension.ClearRegistrationLog();
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    [Fact]
    public void Decorator_AppliesInCorrectOrder()
    {
        _services.AddServices(t => t == typeof(BaseDecoratedService), typeof(BaseDecoratedService));
        var provider = _services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDecoratedService>();

        // Decorators applied descending by order: order 1 (Inner) wraps base, then order 0 (Outer) wraps that
        Assert.Equal("Outer(Inner(Base))", service.Execute());
    }

    [Fact]
    public void Decorator_ResolvedServiceIsOuterDecorator()
    {
        _services.AddServices(t => t == typeof(BaseDecoratedService), typeof(BaseDecoratedService));
        var provider = _services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDecoratedService>();

        Assert.IsType<OuterDecorator>(service);
    }

    [Fact]
    public void Decorator_InfoRecordedInRegistrationLog()
    {
        _services.AddServices(t => t == typeof(BaseDecoratedService), typeof(BaseDecoratedService));

        var registrations = ServicesExtension.GetRegisteredServices(
            serviceType: typeof(IDecoratedService));

        var reg = Assert.Single(registrations);
        Assert.Equal(2, reg.Decorators.Count);
        Assert.Contains(reg.Decorators, d => d.DecoratorType == typeof(InnerDecorator));
        Assert.Contains(reg.Decorators, d => d.DecoratorType == typeof(OuterDecorator));
    }
}
