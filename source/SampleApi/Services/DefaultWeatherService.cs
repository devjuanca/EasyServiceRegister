using EasyServiceRegister.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Services
{
    [RegisterAsScoped]
    [DecorateWith(typeof(CachingWeatherDecorator), order: 1)]
    [DecorateWith(typeof(LoggingWeatherDecorator), order: 0)]
    public class DefaultWeatherService : IWeatherService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
        {
            // Simulate some async work
            await Task.Delay(100);

            var rng = new Random();
            return Enumerable.Range(1, days).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}