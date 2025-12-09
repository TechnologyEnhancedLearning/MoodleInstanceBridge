using Microsoft.AspNetCore.Mvc;

namespace MoodleInstanceBridge.Controllers
{
    [ApiController]

    // API Version 2
    [ApiVersion("2.0")]

    // Route includes version segment
    [Route("api/v{version:apiVersion}/weatherforecast")]
    public class WeatherForecastV2Controller : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm",
            "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet(Name = "GetWeatherForecastV2")]
        public IActionResult Get()
        {
            var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            });

            return Ok(new
            {
                ApiVersion = "2.0",
                ServerTimeUtc = DateTime.UtcNow,
                Forecast = forecast
            });
        }
    }
}
