using Microsoft.Extensions.Caching.Memory;

namespace EasyServiceRegister.Sample.Services.DecoratorSample;

public class CachingWeatherDecorator(IWeatherService weatherService, IMemoryCache cache) : IWeatherService
{
    private const string CacheKeyPrefix = "WeatherForecast_";

    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
    {
        string cacheKey = $"{CacheKeyPrefix}{days}";

        if (!cache.TryGetValue(cacheKey, out IEnumerable<WeatherForecast>? forecast))
        {
            forecast = await weatherService.GetForecastAsync(days);

            cache.Set(cacheKey, forecast, TimeSpan.FromMinutes(5));
        }

        return forecast ?? [];
    }
}