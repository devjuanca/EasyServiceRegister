namespace Application.Interfaces;

public interface IAppSettingsService
{
    string this[string key] { get; }
}
