using System.ComponentModel;

namespace EasyServiceRegister.Attributes;

/// <summary>
/// Mark a service class to be registered in IoC as Scoped.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterAsScopedAttribute : Attribute
{
    /// <summary>
    /// Defines how to register the service by using TryAddScoped or AddScoped, default value is false.
    /// </summary>
    public bool UseTryAddScoped { get; init; }

    public RegisterAsScopedAttribute(bool useTryAddScoped = false)
    {
        UseTryAddScoped = useTryAddScoped;
    }
}
