using EasyServiceRegister.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.Tests;

public class TryAddTests : IDisposable
{
    private readonly ServiceCollection _services;

    public TryAddTests()
    {
        ServicesExtension.ClearRegistrationLog();
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    [Fact]
    public void TryAdd_OnlyFirstRegistrationWins()
    {
        _services.AddServices(
            t => t == typeof(FirstTryAddService) || t == typeof(SecondTryAddService),
            typeof(FirstTryAddService));
        var provider = _services.BuildServiceProvider();

        var service = provider.GetRequiredService<ITryAddService>();

        // TryAdd means the first registration wins, second is skipped
        Assert.IsType<FirstTryAddService>(service);
        Assert.Equal("First", service.Value);
    }

    [Fact]
    public void TryAdd_LogsCorrectRegistrationMethod()
    {
        _services.AddServices(
            t => t == typeof(FirstTryAddService) || t == typeof(SecondTryAddService),
            typeof(FirstTryAddService));

        var registrations = ServicesExtension.GetRegisteredServices(
            serviceType: typeof(ITryAddService));

        Assert.All(registrations, r => Assert.Equal("TryAdd", r.RegistrationMethod));
    }
}
