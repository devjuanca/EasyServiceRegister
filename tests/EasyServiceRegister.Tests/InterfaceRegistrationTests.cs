using EasyServiceRegister.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.Tests;

public class InterfaceRegistrationTests : IDisposable
{
    private readonly ServiceCollection _services;

    public InterfaceRegistrationTests()
    {
        ServicesExtension.ClearRegistrationLog();
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    [Fact]
    public void ExplicitInterface_RegistersAgainstSpecifiedInterface()
    {
        _services.AddServices(t => t == typeof(ExplicitInterfaceService), typeof(ExplicitInterfaceService));
        var provider = _services.BuildServiceProvider();

        var service = provider.GetRequiredService<IExplicitInterfaceService>();

        Assert.IsType<ExplicitInterfaceService>(service);
        Assert.Equal("Explicit", service.Name);
    }

    [Fact]
    public void SelfRegistration_RegistersConcreteType()
    {
        _services.AddServices(t => t == typeof(SelfRegisteredService), typeof(SelfRegisteredService));
        var provider = _services.BuildServiceProvider();

        var service = provider.GetRequiredService<SelfRegisteredService>();

        Assert.NotNull(service);
        Assert.Equal("SelfRegistered", service.Value);
    }

    [Fact]
    public void RegisterAsAllInterfaces_RegistersAgainstEveryInterface()
    {
        _services.AddServices(t => t == typeof(AllInterfacesService), typeof(AllInterfacesService));
        var provider = _services.BuildServiceProvider();

        var asA = provider.GetRequiredService<IAllInterfacesA>();
        var asB = provider.GetRequiredService<IAllInterfacesB>();

        Assert.IsType<AllInterfacesService>(asA);
        Assert.IsType<AllInterfacesService>(asB);
    }
}
