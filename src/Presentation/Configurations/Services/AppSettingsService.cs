using Application.Interfaces;
using ServiceInyector.Interfaces;

namespace Presentation.Services;

public class AppSettingsService : IAppSettingsService, IRegisterAsSingleton
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
