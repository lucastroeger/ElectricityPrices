using Microsoft.AspNetCore.Mvc;
namespace ElectricityPricesApp.Controllers;

[Route("weather")]
public class WeatherController : Controller
{
    private readonly OpenMeteoService _weatherService;

    public WeatherController(OpenMeteoService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var weatherData = await _weatherService.GetWeatherForecastAsync(52.52, 13.41);

        if (weatherData == null)
        {
            return View("Error");
        }
        
        return View(weatherData);
    }
}

