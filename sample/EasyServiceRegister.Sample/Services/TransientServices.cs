using System;
using EasyServiceRegister.Attributes;

namespace EasyServiceRegister.Sample.Services;

[RegisterAsTransient]
public class TransientSampleService
{
    private Guid Id { get; } = Guid.NewGuid();

    public string GetId()
    {
        return $"Get Id from TransientSampleService: {Id}";
    }
}