using System.Text.Json.Serialization;

namespace HackZurich2021.WebApi.Model.Weather
{
    public class WeatherDetails
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("main")]
        public string Main { get; set; }
    }
}