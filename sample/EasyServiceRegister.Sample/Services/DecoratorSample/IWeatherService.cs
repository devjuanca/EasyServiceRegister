using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyServiceRegister.Sample.Services.DecoratorSample
{
    public interface IWeatherService
    {
        Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days);
    }
}