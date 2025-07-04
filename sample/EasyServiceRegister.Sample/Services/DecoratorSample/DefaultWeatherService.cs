using EasyServiceRegister.Attributes;

namespace EasyServiceRegister.Sample.Services.DecoratorSample;

[RegisterAsScoped]
[DecorateWith(typeof(LoggingWeatherDecorator), order: 0)]
[DecorateWith(typeof(CachingWeatherDecorator), order: 1)]
public class DefaultWeatherService : IWeatherService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
    {
        await Task.Delay(100);

        var rng = new Random();

        return [.. Enumerable.Range(1, days).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)]
        })];
    }
}