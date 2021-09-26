using System;
using System.Collections.Generic;

namespace HackZurich2021.WebApi.Model.Weather
{
    public interface IWeatherData
    {
        int Clouds { get; set; }
        double DewPoint { get; set; }
        double FeelsLike { get; set; }
        int Humidity { get; set; }
        int Pressure { get; set; }
        double Temperature { get; set; }
        DateTime Timestamp { get; }
        int UnixTicks { get; set; }
        double Uvi { get; set; }
        int Visibility { get; set; }
        List<WeatherDetails> WeatherDetails { get; set; }
        int WindDeg { get; set; }
        double WindGust { get; set; }
        double WindSpeed { get; set; }
    }
}