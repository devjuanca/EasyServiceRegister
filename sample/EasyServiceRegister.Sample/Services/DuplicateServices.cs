using EasyServiceRegister.Attributes;

namespace EasyServiceRegister.Sample.Services;

public interface IDuplicateService
{
    string GetId();
}

[RegisterAsSingleton]
public class FirstDuplicateService : IDuplicateService
{
    public string GetId() => "FirstDuplicateService";
}

[RegisterAsSingleton]
public class SecondDuplicateService : IDuplicateService
{
    public string GetId() => "SecondDuplicateService";
}