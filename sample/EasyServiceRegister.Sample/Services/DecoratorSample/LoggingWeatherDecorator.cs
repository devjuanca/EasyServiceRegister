namespace EasyServiceRegister.Sample.Services.DecoratorSample;

public class LoggingWeatherDecorator(IWeatherService weatherService, ILogger<LoggingWeatherDecorator> logger) : IWeatherService
{
    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
    {
        logger.LogInformation("Getting weather forecast for {Days} days", days);

        var result = await weatherService.GetForecastAsync(days);

        logger.LogInformation("Weather forecast retrieved successfully");

        return result;
    }
}