using EasyServiceRegister.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Services
{
    [RegisterAsScopedKeyed("Europe")]
    [DecorateWith(typeof(MetricConversionDecorator))]
    public class EuropeanWeatherService : IWeatherService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Cold", "Chilly", "Cool", "Mild", "Warm", "Hot", "Sweltering", "Scorching"
        };

        public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
        {
            // Simulate some async work
            await Task.Delay(100);

            var rng = new Random();
            return Enumerable.Range(1, days).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-10, 35), // European temperature range
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }

    [RegisterAsScopedKeyed("NorthAmerica")]
    [DecorateWith(typeof(ImperialConversionDecorator))]
    public class NorthAmericanWeatherService : IWeatherService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Cold", "Cool", "Mild", "Warm", "Hot", "Sweltering", "Scorching"
        };

        public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
        {
            // Simulate some async work
            await Task.Delay(100);

            var rng = new Random();
            return Enumerable.Range(1, days).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 45), // North American temperature range 
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}