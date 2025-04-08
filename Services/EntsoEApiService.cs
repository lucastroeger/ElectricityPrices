using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using ElectricityPricesApp.Models;

public class EntsoEApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public EntsoEApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ApiKeys:EntsoE"] ??
                  throw new ArgumentNullException("EntsoE API key is missing in configuration.");
    }

    public async Task<ElectricityPrices> GetDayAheadPricesAsync(DateTime startDate,
        string biddingZone)
    {
        // See https://transparency.entsoe.eu/content/static_content/Static%20content/web%20api/Guide_prod_backup_06_11_2024.html
        const string baseUrl = "https://web-api.tp.entsoe.eu/api?";
        string start = startDate.ToString("yyyyMMddHHmm");
        string end = startDate.AddDays(1).ToString("yyyyMMddHHmm");

        string requestUrl = $"{baseUrl}" +
                            $"documentType=A44" +
                            $"&periodStart={start}" +
                            $"&periodEnd={end}" +
                            $"&out_Domain={biddingZone}" +
                            $"&in_Domain={biddingZone}" +
                            $"&securityToken={_apiKey}";
        
        Console.WriteLine(requestUrl);

        HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error fetching data: {response.StatusCode}");
        }

        string xmlContent = await response.Content.ReadAsStringAsync();
        return ParsePrices(xmlContent);
    }

    private ElectricityPrices ParsePrices(string xmlContent)
    {
        XDocument xmlDoc = XDocument.Parse(xmlContent);
        XNamespace ns = xmlDoc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Extract all TimeSeries elements
        var timeSeriesList = xmlDoc.Descendants(ns + "TimeSeries").ToList();

        if (!timeSeriesList.Any())
        {
            Console.WriteLine("No <TimeSeries> elements found in XML.");
            return new ElectricityPrices { Prices = new List<ElectricityPrice>() };
        }

        Console.WriteLine($"Found {timeSeriesList.Count} <TimeSeries> elements.");

        // Select the first TimeSeries with PT15M resolution.
        // NOTE: The first one gives the data for the current day, the second one gives the data for the following day
        var timeSeries = timeSeriesList
            .FirstOrDefault(ts => ts.Descendants(ns + "Period")
            .Any(period => (period.Element(ns + "resolution")?.Value ?? "") == "PT15M"));

        if (timeSeries == null)
        {
            Console.WriteLine("No 15-minute resolution data found.");
            return new ElectricityPrices { Prices = new List<ElectricityPrice>() };
        }
        
        var periodElement = timeSeries.Descendants(ns + "Period")
            .FirstOrDefault(period => (period.Element(ns + "resolution")?.Value ?? "") == "PT15M");

        if (periodElement == null)
        {
            Console.WriteLine("Error: Could not find Period with PT15M.");
            return new ElectricityPrices { Prices = new List<ElectricityPrice>() };
        }

        // Extract start time from <timeInterval>
        DateTime startTime;
        var timeInterval = periodElement.Element(ns + "timeInterval");
        if (timeInterval != null)
        {
            startTime = DateTime.Parse(timeInterval.Element(ns + "start")?.Value    // This seems to be parsed to UTC+1
                ?? throw new Exception("Start time missing"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }
        else
        {
            throw new Exception("Missing <timeInterval> in TimeSeries.");
        }
        
        Console.WriteLine(timeInterval.Element(ns + "start")?.Value);
        Console.WriteLine($"⏳ Start Time: {startTime}");

        // Extract prices from <Point> elements
        var prices = periodElement.Descendants(ns + "Point")
            .Select(point =>
            {
                int position = int.Parse(point.Element(ns + "position")?.Value ?? "1");
                decimal price = decimal.Parse(point.Element(ns + "price.amount")?.Value ?? "0", CultureInfo.InvariantCulture);
                return new ElectricityPrice
                {
                    Timestamp = startTime.AddMinutes((position - 1) * 15),
                    Price = price
                };
            })
            .ToList();

        Console.WriteLine($"Found: {prices.Count} prices");
        
        // Console.WriteLine("\n📊 Parsed Prices:");
        // foreach (var price in prices)
        // {
        //     Console.WriteLine($"- Timestamp: {price.Timestamp}, Price: {price.Price} EUR/MWh");
        // }

        return new ElectricityPrices
        {
            Prices = prices,
            Date = startTime
        };
    }
}