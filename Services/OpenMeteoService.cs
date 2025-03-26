using System.Globalization;
using Newtonsoft.Json;

public class OpenMeteoService
{
    private readonly HttpClient _httpClient;

    public OpenMeteoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherForecast> GetWeatherForecastAsync(string latitude, string longitude)
    {
        // Data from https://open-meteo.com/en/docs
        string requestUrl = $"https://api.open-meteo.com/v1/forecast?" +
                            $"latitude={latitude}&longitude={longitude}" +
                            $"&minutely_15=temperature_2m,wind_speed_10m,shortwave_radiation" +
                            $"&forecast_days=3";

        Console.WriteLine(requestUrl);
        
        // Send a GET request to the API
        var response = await _httpClient.GetStringAsync(requestUrl);
        
        // Deserialize the JSON response into a WeatherForecast object
        var weatherForecast = JsonConvert.DeserializeObject<WeatherForecast>(response);
        
        return weatherForecast;
    }
}