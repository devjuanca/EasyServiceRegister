using EasyServiceRegister.Attributes;

namespace EasyServiceRegister.Sample.Services;

[RegisterAsScoped]
public class ScopedSampleService
{
    private Guid Id { get; } = Guid.NewGuid();

    public string GetId()
    {
        return $"Get Id from ScopedSampleService: {Id}";
    }
}

[RegisterAsScopedKeyed("KeyedScopedSampleServices")]
public class KeyedScopedSampleServices
{
    private Guid Id { get; } = Guid.NewGuid();

    public string GetId()
    {
        return $"Get Id from KeyedScopedSampleServices: {Id}";
    }
}

public interface IScopedSampleService
{
    string GetId();
}

[RegisterAsScoped]
public class ScopedSampleServiceImpl : IScopedSampleService
{
    private Guid Id { get; } = Guid.NewGuid();

    public string GetId()
    {
        return $"Get Id from ScopedSampleServiceImpl: {Id}";
    }
}