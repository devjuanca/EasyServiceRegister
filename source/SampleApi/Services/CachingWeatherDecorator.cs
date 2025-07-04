using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApi.Services
{
    public class CachingWeatherDecorator : IWeatherService
    {
        private readonly IWeatherService _weatherService;
        private readonly IMemoryCache _cache;
        private const string CacheKeyPrefix = "WeatherForecast_";

        public CachingWeatherDecorator(
            IWeatherService weatherService,
            IMemoryCache cache)
        {
            _weatherService = weatherService;
            _cache = cache;
        }

        public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
        {
            string cacheKey = $"{CacheKeyPrefix}{days}";
            
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<WeatherForecast> forecast))
            {
                forecast = await _weatherService.GetForecastAsync(days);
                
                // Cache for 5 minutes
                _cache.Set(cacheKey, forecast, TimeSpan.FromMinutes(5));
            }
            
            return forecast;
        }
    }
}