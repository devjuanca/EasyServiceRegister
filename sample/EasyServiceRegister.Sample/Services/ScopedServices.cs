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

[RegisterAsScoped(serviceInterface: typeof(IScopedSampleService))]
public class ScopedSampleServiceImpl : IScopedSampleService, IDisposable
{
    private Guid Id { get; } = Guid.NewGuid();

    public string GetId()
    {
        return $"Get Id from ScopedSampleServiceImpl: {Id}";
    }

    public void Dispose()
    {
        return;
    }
}