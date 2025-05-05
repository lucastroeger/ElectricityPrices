using System;
using System.Collections.Generic;

namespace ElectricityPricesApp.Models
{
    public class DashboardViewModel
    {
        public ElectricityPrices ElectricityPrices { get; set; }
        public WeatherForecast WeatherForecast { get; set; }
        public DateTime Date { get; set; }
        public string SelectedLocation { get; set; }
        public List<Location> AvailableLocations { get; set; }
    }
} 