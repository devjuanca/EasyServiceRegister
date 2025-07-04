using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApi.Services
{
    public interface IWeatherService
    {
        Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days);
    }
}