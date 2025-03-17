using Microsoft.AspNetCore.Mvc;
using ElectricityPricesApp.Models;
namespace ElectricityPricesApp.Controllers;

public class HomeController : Controller
{
    private readonly SmardApiService _smardApiService;

    public HomeController(SmardApiService smardApiService)
    {
        _smardApiService = smardApiService;
    }
    // Default GET request to show the form and default data
    public async Task<IActionResult> Index()
    {
        DateTime startDate = DateTime.UtcNow;
        ElectricityPrices foundPrices = await _smardApiService.GetElectricityPricesAsync(startDate.AddDays(-1), startDate);
        return View(foundPrices);
    }
    
    // POST request when the user submits the date range
    [HttpPost]
    public async Task<IActionResult> Index(DateRangeModel dateRange)
    {
        ElectricityPrices foundPrices = await _smardApiService.GetElectricityPricesAsync(dateRange.StartDate, dateRange.EndDate);
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
