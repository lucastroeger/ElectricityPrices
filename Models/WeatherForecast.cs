using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class WeatherForecast
{
    [JsonProperty("latitude")]
    public double Latitude { get; set; }

    [JsonProperty("longitude")]
    public double Longitude { get; set; }

    [JsonProperty("generationtime_ms")]
    public double GenerationTimeMs { get; set; }

    [JsonProperty("timezone")]
    public string Timezone { get; set; }

    [JsonProperty("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; set; }

    [JsonProperty("elevation")]
    public double Elevation { get; set; }

    [JsonProperty("minutely_15")]
    public MinutelyData Minutely15 { get; set; }
}

public class MinutelyData
{
    [JsonProperty("time")]
    public List<DateTime> Time { get; set; }

    [JsonProperty("temperature_2m")]
    public List<double> Temperature2m { get; set; }

    [JsonProperty("wind_speed_10m")]
    public List<double> WindSpeed10m { get; set; }

    [JsonProperty("shortwave_radiation")]
    public List<double> ShortwaveRadiation { get; set; }
}