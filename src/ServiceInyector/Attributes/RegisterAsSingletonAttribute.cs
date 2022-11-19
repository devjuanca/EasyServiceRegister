namespace EasyServiceRegister.Attributes;

/// <summary>
/// Mark a service class to be registered in IoC as Singleton.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterAsSingletonAttribute : Attribute
{
    /// <summary>
    /// Defines how to register the service by using TryAddSingleton or AddSingleton, default value is false.
    /// </summary>
    public bool UseTryAddSingleton { get; init; }
    public RegisterAsSingletonAttribute(bool useTryAddSingleton = false)
    {
        UseTryAddSingleton = useTryAddSingleton;
    }
}
