using Microsoft.AspNetCore.Mvc;
using ElectricityPricesApp.Models;
namespace ElectricityPricesApp.Controllers;

public class HomeController : Controller
{
    private readonly SmardApiService _smardApiService;
    private readonly EntsoEApiService _entsoEApiService;
    public HomeController(SmardApiService smardApiService, EntsoEApiService entsoEApiService)
    {
        _smardApiService = smardApiService;
        _entsoEApiService = entsoEApiService;
    }
    
    // Default GET request to show the form and default data
    public async Task<IActionResult> Index()
    {
        DateTime startDate = DateTime.UtcNow;
        ElectricityPrices foundPrices = await _entsoEApiService.GetDayAheadPricesAsync(startDate,"10YAT-APG------L");
        return View(foundPrices);
    }
    
    // POST request when the user submits the date range
    [HttpPost]
    public async Task<IActionResult> Index(DateRangeModel dateRange)
    {
        ElectricityPrices foundPrices = null;
        string dataSource = "entsoe";
        
        if (dataSource == "smard")
        {
            foundPrices = await _smardApiService.GetElectricityPricesAsync(dateRange.StartDate, dateRange.EndDate);
        }
        else if (dataSource == "entsoe")
        {
            foundPrices = await _entsoEApiService.GetDayAheadPricesAsync(dateRange.StartDate, "10YAT-APG------L");
        }
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
