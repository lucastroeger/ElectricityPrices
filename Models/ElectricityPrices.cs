namespace ElectricityPricesApp.Models
{
    public class ElectricityPrices
    {
        public string? CountryCode { get; init; }  // E.g., "10YDE-VE-------2" for Germany. See https://de.wikipedia.org/wiki/Energy_Identification_Code
        public DateTime Date { get; init; }
        public List<ElectricityPrice>? Prices { get; init; }
    }
}