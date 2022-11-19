using Application.Interfaces;
using ServiceInyector.Attributes;

namespace Presentation.Services;

[RegisterAsSingleton]
public class AppSettingsService : IAppSettingsService
{
    private readonly IConfiguration _configuration;

    public AppSettingsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string this[string key]
    {
        get => _configuration[key];
    }
}
