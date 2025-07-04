using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Services
{
    public class MetricConversionDecorator : IWeatherService
    {
        private readonly IWeatherService _weatherService;

        public MetricConversionDecorator(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
        {
            var forecasts = await _weatherService.GetForecastAsync(days);
            
            // No conversion needed as it's already in Celsius
            // But we could add some European-specific formatting or adjustments
            return forecasts.Select(f => 
            {
                f.Summary = $"{f.Summary} (Metric)";
                return f;
            });
        }
    }

    public class ImperialConversionDecorator : IWeatherService
    {
        private readonly IWeatherService _weatherService;

        public ImperialConversionDecorator(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
        {
            var forecasts = await _weatherService.GetForecastAsync(days);
            
            // The TemperatureF property already does the conversion
            // But we could add some North American-specific formatting
            return forecasts.Select(f => 
            {
                f.Summary = $"{f.Summary} (Imperial)";
                return f;
            });
        }
    }
}