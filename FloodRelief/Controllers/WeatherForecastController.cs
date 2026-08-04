using FloodRelief.Services.Weather;
using Microsoft.AspNetCore.Mvc;

namespace FloodRelief.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly WeatherForecastService _service;
        public WeatherForecastController(WeatherForecastService service) => _service = service;

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get() => _service.GetForecast();
    }
}
