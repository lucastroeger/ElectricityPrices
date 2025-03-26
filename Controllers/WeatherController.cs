using System.Globalization;
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
    
    public async Task<IActionResult> Index()
    {
        // Use default coordinates (for example, Berlin)
        string latitude = "52.52";
        string longitude = "13.405";
        
        var weatherData = await _weatherService.GetWeatherForecastAsync(latitude, longitude);

        if (weatherData == null)
        {
            return View("Error");
        }
        
        return View(weatherData);
    }
    
    [HttpPost]
    public async Task<IActionResult> Index(string latitude, string longitude)
    {

        var weatherData = await _weatherService.GetWeatherForecastAsync(latitude, longitude);

        if (weatherData == null)
        {
            return View("Error");
        }
        
        return View(weatherData);
    }
}

