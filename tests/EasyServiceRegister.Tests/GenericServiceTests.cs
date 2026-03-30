using EasyServiceRegister.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyServiceRegister.Tests;

public class GenericServiceTests : IDisposable
{
    private readonly ServiceCollection _services;

    public GenericServiceTests()
    {
        ServicesExtension.ClearRegistrationLog();
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        ServicesExtension.ClearRegistrationLog();
    }

    [Fact]
    public void OpenGeneric_ResolvesForStringType()
    {
        _services.AddServices(t => t.Name.StartsWith("GenericService"), typeof(GenericService<>));
        var provider = _services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IGenericService<string>>();

        Assert.NotNull(service);
        Assert.Equal("hello", service.Process("hello"));
    }

    [Fact]
    public void OpenGeneric_ResolvesForDifferentTypes()
    {
        _services.AddServices(t => t.Name.StartsWith("GenericService"), typeof(GenericService<>));
        var provider = _services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var stringService = scope.ServiceProvider.GetRequiredService<IGenericService<string>>();
        var objectService = scope.ServiceProvider.GetRequiredService<IGenericService<object>>();

        Assert.NotNull(stringService);
        Assert.NotNull(objectService);
        Assert.NotSame(stringService, objectService);
    }
}
