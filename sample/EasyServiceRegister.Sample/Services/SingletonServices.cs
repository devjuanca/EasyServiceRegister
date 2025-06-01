using System;
using EasyServiceRegister.Attributes;

namespace EasyServiceRegister.Sample.Services;

[RegisterAsSingleton]
public class SingletonSampleService
{
    private Guid Id { get; } = Guid.NewGuid();

    public string GetId()
    {
        return $"Get Id from SingletonSampleService: {Id}";
    }
}
