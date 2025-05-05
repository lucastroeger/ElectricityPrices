using Microsoft.AspNetCore.Mvc;
using ElectricityPricesApp.Models;
namespace ElectricityPricesApp.Controllers;

public class HomeController : Controller
{
    private readonly EntsoEApiService _entsoEApiService;
    private readonly OpenMeteoService _openMeteoService;
    private readonly Dictionary<string, (string name, List<Location> locations)> _biddingZoneLocations = new()
    {
        { 
            "10Y1001A1001A82H", 
            ("Germany-Luxembourg (BZN|DE-LU)", 
            new List<Location>
            {
                new() { Name = "Frankfurt", Latitude = "50.1109", Longitude = "8.6821" },
                new() { Name = "Berlin", Latitude = "52.5200", Longitude = "13.4050" },
                new() { Name = "Munich", Latitude = "48.1351", Longitude = "11.5820" },
                new() { Name = "Luxembourg City", Latitude = "49.6116", Longitude = "6.1319" }
            })
        },
        { 
            "10YAT-APG------L", 
            ("Austria (BZN|AT)",
            new List<Location>
            {
                new() { Name = "Vienna", Latitude = "48.2082", Longitude = "16.3738" },
                new() { Name = "Graz", Latitude = "47.0707", Longitude = "15.4395" },
                new() { Name = "Linz", Latitude = "48.3069", Longitude = "14.2858" },
                new() { Name = "Salzburg", Latitude = "47.8095", Longitude = "13.0550" }
            })
        }
    };

    public HomeController(EntsoEApiService entsoEApiService, OpenMeteoService openMeteoService)
    {
        _entsoEApiService = entsoEApiService;
        _openMeteoService = openMeteoService;
    }
    
    // Default GET request to show the form and default data
    public async Task<IActionResult> Index()
    {
        DateTime startDate = DateTime.UtcNow;
        string biddingZone = "10Y1001A1001A82H";
        
        var (name, locations) = _biddingZoneLocations[biddingZone];
        var electricityPrices = await _entsoEApiService.GetDayAheadPricesAsync(startDate, biddingZone);
        var weatherForecast = await _openMeteoService.GetWeatherForecastAsync(locations[0].Latitude, locations[0].Longitude);
        
        ViewData["BiddingZone"] = name;
        
        return View(new DashboardViewModel
        {
            ElectricityPrices = electricityPrices,
            WeatherForecast = weatherForecast,
            Date = startDate,
            SelectedLocation = locations[0].Name,
            AvailableLocations = locations
        });
    }
    
    // POST request when the user submits the date range
    [HttpPost]
    public async Task<IActionResult> Index(DateRangeModel dateRange)
    {
        var (name, locations) = _biddingZoneLocations[dateRange.BiddingZone];
        var electricityPrices = await _entsoEApiService.GetDayAheadPricesAsync(dateRange.StartDate, dateRange.BiddingZone);
        
        // Find the selected location or default to the first one
        var selectedLocation = locations.FirstOrDefault(l => l.Name == dateRange.SelectedLocation) ?? locations[0];
        var weatherForecast = await _openMeteoService.GetWeatherForecastAsync(selectedLocation.Latitude, selectedLocation.Longitude);
        
        ViewData["BiddingZone"] = name;
        
        return View(new DashboardViewModel
        {
            ElectricityPrices = electricityPrices,
            WeatherForecast = weatherForecast,
            Date = dateRange.StartDate,
            SelectedLocation = selectedLocation.Name,
            AvailableLocations = locations
        });
    }
    
    public IActionResult About()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
