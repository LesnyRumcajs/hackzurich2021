using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HackZurich2021.WebApi.Model.Weather
{
    public class CurrentWeatherData : IWeatherData
    {
        [JsonPropertyName("clouds")]
        public int Clouds { get; set; }

        [JsonPropertyName("dew_point")]
        public double DewPoint { get; set; }

        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }

        [JsonPropertyName("pressure")]
        public int Pressure { get; set; }

        [JsonPropertyName("sunrise")]
        public int Sunrise { get; set; }

        [JsonPropertyName("sunset")]
        public int Sunset { get; set; }

        [JsonPropertyName("temp")]
        public double Temperature { get; set; }

        public DateTime Timestamp { get => TimestampConverter.UnixTimeStampToDateTime(UnixTicks); }

        [JsonPropertyName("dt")]
        public int UnixTicks { get; set; }

        [JsonPropertyName("uvi")]
        public double Uvi { get; set; }

        [JsonPropertyName("visibility")]
        public int Visibility { get; set; }

        [JsonPropertyName("weather")]
        public List<WeatherDetails> WeatherDetails { get; set; }

        [JsonPropertyName("wind_deg")]
        public int WindDeg { get; set; }

        [JsonPropertyName("wind_gust")]
        public double WindGust { get; set; }

        [JsonPropertyName("wind_speed")]
        public double WindSpeed { get; set; }
    }
}