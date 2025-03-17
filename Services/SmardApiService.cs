using System;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ElectricityPricesApp.Models;

public class SmardApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://smard.api.proxy.bund.dev/app/chart_data/4169/DE/4169_DE_quarterhour_"; // https://smard.api.bund.dev/

    public SmardApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ElectricityPrices> GetElectricityPricesAsync(DateTime startDate, DateTime endDate)
    {
        // Convert startDate and endDate to Unix timestamps (milliseconds)
        var startUnixTimestamp = new DateTimeOffset(startDate).ToUnixTimeMilliseconds();
        var endUnixTimestamp = new DateTimeOffset(endDate).ToUnixTimeMilliseconds();
        Console.WriteLine($"Given timestamp: {startUnixTimestamp}");
        
        try
        {
            // Fetch available timestamps and compute closest value from startDate
            string timestampUrl = "https://smard.api.proxy.bund.dev/app/chart_data/4169/DE/index_quarterhour.json";
            HttpResponseMessage responseTimestamps = await _httpClient.GetAsync(timestampUrl);

            if (!responseTimestamps.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch timestamp data from SMARD.");
            }

            // Read response as JSON string
            var jsonStringTimestamps = await responseTimestamps.Content.ReadAsStringAsync();

            // Parse JSON
            var jsonDataTimestamps = JsonDocument.Parse(jsonStringTimestamps);

            // Extract the timestamps array
            var timestampsArray = jsonDataTimestamps.RootElement.GetProperty("timestamps");

            // Convert timestamps to a List<long> (Unix timestamps in milliseconds)
            List<long> timestamps = timestampsArray.EnumerateArray()
                .Select(ts => ts.GetInt64()) // Keep as long (milliseconds)
                .ToList();
            
            startUnixTimestamp = timestamps.OrderBy(item => Math.Abs(startUnixTimestamp - item)).First();
            Console.WriteLine($"Chosen timestamp: {startUnixTimestamp}");
            
            // Fetch time series for the given timestamp
            string requestUrl = $"{BaseUrl}{startUnixTimestamp}.json"; 

            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch data: {response.StatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonData = JsonDocument.Parse(jsonString);

            // Check if "series" property exists
            if (!jsonData.RootElement.TryGetProperty("series", out JsonElement series) || series.ValueKind != JsonValueKind.Array)
            {
                throw new Exception("API response does not contain expected data.");
            }

            var prices = new List<ElectricityPrice>();

            foreach (var entry in series.EnumerateArray())
            {
                if (entry.GetArrayLength() < 2) continue;

                var timestampMs = entry[0].GetInt64();
                var priceValue = entry[1].GetDecimal();

                // Convert timestamp to DateTime
                var timestampUtc = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs).UtcDateTime;

                prices.Add(new ElectricityPrice
                {
                    Timestamp = timestampUtc,
                    Price = priceValue
                });
            }
            
            return new ElectricityPrices
            {
                CountryCode = "10YDE-VE-------2", // Use the correct market ID
                Date = DateTimeOffset.FromUnixTimeMilliseconds(startUnixTimestamp).UtcDateTime,
                Prices = prices            
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching electricity prices: {ex.Message}");
            return new ElectricityPrices { Prices = new List<ElectricityPrice>() };
        }
            
    }
}