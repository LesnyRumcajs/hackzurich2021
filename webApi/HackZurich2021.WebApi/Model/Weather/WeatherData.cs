using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HackZurich2021.WebApi.Model.Weather
{
    public class WeatherData
    {
        [JsonPropertyName("current")]
        public CurrentWeatherData CurrentWeatherData { get; set; }

        [JsonPropertyName("hourly")]
        public List<HourlyWeatherData> HourlyWeatherData { get; set; }

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("timezone_offset")]
        public int TimezoneOffset { get; set; }
    }
}