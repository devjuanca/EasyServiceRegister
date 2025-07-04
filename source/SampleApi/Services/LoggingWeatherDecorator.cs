using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApi.Services
{
    public class LoggingWeatherDecorator : IWeatherService
    {
        private readonly IWeatherService _weatherService;
        private readonly ILogger<LoggingWeatherDecorator> _logger;

        public LoggingWeatherDecorator(
            IWeatherService weatherService, 
            ILogger<LoggingWeatherDecorator> logger)
        {
            _weatherService = weatherService;
            _logger = logger;
        }

        public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
        {
            _logger.LogInformation("Getting weather forecast for {Days} days", days);
            
            var result = await _weatherService.GetForecastAsync(days);
            
            _logger.LogInformation("Weather forecast retrieved successfully");
            return result;
        }
    }
}