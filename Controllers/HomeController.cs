using Microsoft.AspNetCore.Mvc;
using ElectricityPricesApp.Models;
namespace ElectricityPricesApp.Controllers;

public class HomeController : Controller
{
    private readonly EntsoEApiService _entsoEApiService;
    public HomeController(EntsoEApiService entsoEApiService)
    {
        _entsoEApiService = entsoEApiService;
    }
    
    // Default GET request to show the form and default data
    public async Task<IActionResult> Index()
    {
        DateTime startDate = DateTime.UtcNow;
        string biddingZone = "10Y1001A1001A82H"; // BZN|DE-LU
        ElectricityPrices foundPrices = await _entsoEApiService.GetDayAheadPricesAsync(startDate, biddingZone);
        ViewData["BiddingZone"] = biddingZone;
        
        return View(foundPrices);
    }
    
    // POST request when the user submits the date range
    [HttpPost]
    public async Task<IActionResult> Index(DateRangeModel dateRange)
    {
         ElectricityPrices foundPrices = await _entsoEApiService.GetDayAheadPricesAsync(dateRange.StartDate, dateRange.BiddingZone);
        
        ViewData["BiddingZone"] = dateRange.BiddingZone;
        
        return View(foundPrices);
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
