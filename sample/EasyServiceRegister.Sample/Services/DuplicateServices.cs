using EasyServiceRegister.Attributes;

namespace EasyServiceRegister.Sample.Services;

//This code will generate a warning due to duplicate service registrations.

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