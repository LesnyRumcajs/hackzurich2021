using HackZurich2021.WebApi.Model;
using HackZurich2021.WebApi.Model.Weather;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace HackZurich2021.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnomalyEvaluationController : ControllerBase
    {
        public AnomalyEvaluationController(ILogger<InfluxDbController> logger)
        {
            _logger = logger;

            InitializeInfluxDbClient();
            InitializeHttpClient();
        }

        public enum ErrorCategories
        {
            OK,
            LoopAntennaBroken,
            LoopAntennaBadCondition,
            CriticalInterference,
            PossibleInterference
        }

        [HttpPost]
        [Route(nameof(CreateAnomalyEvaluation))]
        public List<string> CreateAnomalyEvaluation(DateTime anomalyTimestamp, int timelapse = 10, int signalLimit = 1, int badTelegramThreshold = 2, int warningTelegramThreshold = 4)
        {
            var results = new List<string>();
            SetCulture();

            var start = anomalyTimestamp.Subtract(new TimeSpan(0, 0, timelapse));
            var end = anomalyTimestamp.Add(new TimeSpan(0, 0, timelapse));
            var query = $"from(bucket: \"{_settings.Bucket}\") |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ}) |> filter(fn: (r) => r[\"category\"] == \"rssi\" " +
                $"or r[\"category\"] == \"position\" or r[\"category\"] == \"telegram\")";
            var raw = _client.GetQueryApi().QueryAsync(query, _settings.Organization).Result;

            var dataSet = ExtractDataFromFluxTables(raw);

            // Find bad/warning signals and correlate them with telegram data
            var badSignals = new List<FullDataSet>(dataSet.FindAll(d => d.RssiData.Quality is RssiData.RssiDataQuality.Weak or RssiData.RssiDataQuality.Undefined));
            var warningSignals = new List<FullDataSet>(dataSet.FindAll(d => d.RssiData.Quality is RssiData.RssiDataQuality.Fair));

            if (badSignals.Count > signalLimit
                || warningSignals.Count > signalLimit)
            {
                var correlations = CorrelateSignalStrengthAndTelegrams(dataSet, badSignals, warningSignals, signalLimit, badTelegramThreshold, warningTelegramThreshold, out ErrorCategories result);
                if (result != ErrorCategories.OK)
                {
                    foreach (var correlation in correlations)
                    {
                        results.Add($"{result} - {correlation.Key} - {correlation.Value.Print()}");
                    }
                }
                results.AddRange(AnalyzeWeatherConditions(badSignals.Count > 0 ? badSignals.FirstOrDefault() : warningSignals.FirstOrDefault()));
            }

            return results;
        }

        [HttpPost]
        [Route(nameof(CreateAnomalyEvaluationFromAlert))]
        public List<string> CreateAnomalyEvaluationFromAlert()
        {
            using (var r = new StreamReader(Request.Body))
            {
                if (DateTime.TryParse(r.ReadToEnd(), out DateTime timestamp))
                {
                    return CreateAnomalyEvaluation(timestamp);
                }
            }

            return new List<string>() { "Invalid timestamp" };
        }

        private readonly HttpClient _httpClient = new();
        private readonly ILogger<InfluxDbController> _logger;

        private InfluxDBClient _client;

        private InfluxDbSettings _settings;

        private List<string> AnalyzeWeatherConditions(FullDataSet dataSet)
        {
            var results = new List<string>();

            var apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_APIKEY");
            var unixTicks = ((DateTimeOffset)dataSet.Timestamp).ToUnixTimeSeconds();

            var response = _httpClient.GetAsync($@"https://api.openweathermap.org/data/2.5/onecall/timemachine?dt={unixTicks}&appid={apiKey}&lat={dataSet.PositionData.Latitude}&lon={dataSet.PositionData.Longitude}").Result;
            var weatherData = JsonSerializer.Deserialize<WeatherData>(response.Content.ReadAsStringAsync().Result);

            IWeatherData selectedWeatherData;
            if (weatherData.CurrentWeatherData.Timestamp > dataSet.Timestamp.Value.AddHours(1))
            {
                selectedWeatherData = weatherData.HourlyWeatherData.FirstOrDefault(wd => wd.Timestamp < dataSet.Timestamp.Value.AddHours(1));
            }
            else
            {
                selectedWeatherData = weatherData.CurrentWeatherData;
            }

            var temperature = ConvertFahrenheitToCelsius(selectedWeatherData.Temperature);
            var dewPoint = ConvertFahrenheitToCelsius(selectedWeatherData.DewPoint);
            if (temperature <= dewPoint)
            {
                results.Add($"Temperature was below or equal to dew point ({temperature}°C <= {dewPoint}°C)");
            }

            if (temperature <= 4)
            {
                results.Add($"Temperature was below freezing point ({temperature}°C <= 4°C)");
            }

            if (selectedWeatherData.Humidity >= 80)
            {
                results.Add($"Humidity was high ({selectedWeatherData.Humidity}% >= {80}%)");
            }

            if (selectedWeatherData.WindSpeed >= 75)
            {
                results.Add($"Wind speed was high ({selectedWeatherData.WindSpeed}km/h >= {75}km/h)");
            }

            return results;
        }

        private double ConvertFahrenheitToCelsius(double fahrenheit)
        {
            return (fahrenheit - 32) * 5 / 9;
        }

        private Dictionary<DateTime?, TelegramData> CorrelateSignalStrengthAndTelegrams(List<FullDataSet> dataSet, List<FullDataSet> badSignals, List<FullDataSet> warningSignals, int signalLimit,
                                    int badTelegramThreshold, int warningTelegramThreshold, out ErrorCategories result)
        {
            var badCorrelations = new Dictionary<DateTime?, TelegramData>();
            var warningCorrelations = new Dictionary<DateTime?, TelegramData>();
            var badTelegrams = new Dictionary<DateTime?, TelegramData>();
            var warningTelegrams = new Dictionary<DateTime?, TelegramData>();

            for (int i = 1; i < dataSet.Count; i++)
            {
                var previousTelegram = dataSet[i - 1].TelegramData;
                var currentTelegram = dataSet[i].TelegramData;

                var telegramDifference = new TelegramData()
                {
                    A1_Total = currentTelegram.A1_Total - previousTelegram.A1_Total,
                    A1_Valid = currentTelegram.A1_Valid - previousTelegram.A1_Valid,
                    A2_Total = currentTelegram.A2_Total - previousTelegram.A2_Total,
                    A2_Valid = currentTelegram.A2_Valid - previousTelegram.A2_Valid,
                };

                if (telegramDifference.A1_Total < badTelegramThreshold
                    || telegramDifference.A1_Valid < badTelegramThreshold
                    || telegramDifference.A2_Total < badTelegramThreshold
                    || telegramDifference.A2_Valid < badTelegramThreshold)
                {
                    if (badSignals.Any(d => d.Timestamp == dataSet[i].Timestamp))
                    {
                        badCorrelations.Add(dataSet[i].Timestamp, telegramDifference);
                    }
                    else
                    {
                        badTelegrams.Add(dataSet[i].Timestamp, telegramDifference);
                    }
                }
                else if (telegramDifference.A1_Total < warningTelegramThreshold
                    || telegramDifference.A1_Valid < warningTelegramThreshold
                    || telegramDifference.A2_Total < warningTelegramThreshold
                    || telegramDifference.A2_Valid < warningTelegramThreshold)
                {
                    if (warningSignals.Any(d => d.Timestamp == dataSet[i].Timestamp))
                    {
                        warningCorrelations.Add(dataSet[i].Timestamp, telegramDifference);
                    }
                    else
                    {
                        warningTelegrams.Add(dataSet[i].Timestamp, telegramDifference);
                    }
                }
            }

            // Print signals/correlations
            if (badCorrelations.Count > 0)
            {
                // Send alarm to database
                result = ErrorCategories.LoopAntennaBroken;
                return badCorrelations;
            }
            else if (warningCorrelations.Count > 0)
            {
                // Send warning to database
                result = ErrorCategories.LoopAntennaBadCondition;
                return warningCorrelations;
            }
            else if (badTelegrams.Count > 0)
            {
                // Look at duration? → Send alarm to database
                result = ErrorCategories.CriticalInterference;
                return badTelegrams;
            }
            else if (warningTelegrams.Count > 0)
            {
                // Look at duration? → Send warning to database
                result = ErrorCategories.PossibleInterference;
                return warningTelegrams;
            }

            result = ErrorCategories.OK;
            return new Dictionary<DateTime?, TelegramData>();
        }

        private List<FullDataSet> ExtractDataFromFluxTables(List<FluxTable> fluxTables)
        {
            var result = new List<FullDataSet>();
            for (int i = 0; i < fluxTables.Max(t => t.Records.Count); i++)
            {
                var dataSet = new FullDataSet();

                foreach (var table in fluxTables)
                {
                    if (table.Records.Count <= i)
                    {
                        continue;
                    }

                    var record = table.Records[i];
                    var measurement = record.GetMeasurement();
                    var field = record.GetField();
                    var value = record.GetValue().ToString();
                    var timestamp = record.GetTimeInDateTime();

                    dataSet.Timestamp = timestamp;

                    switch (measurement)
                    {
                        case "a1_telegram":
                            switch (field)
                            {
                                case "total":
                                    if (long.TryParse(value, out long a1Total))
                                    {
                                        dataSet.TelegramData.A1_Total = a1Total;
                                    }
                                    break;

                                case "valid":
                                    if (long.TryParse(value, out long a1Valid))
                                    {
                                        dataSet.TelegramData.A1_Valid = a1Valid;
                                    }
                                    break;
                            }
                            break;

                        case "a2_telegram":
                            switch (field)
                            {
                                case "total":
                                    if (long.TryParse(value, out long a2Total))
                                    {
                                        dataSet.TelegramData.A2_Total = a2Total;
                                    }
                                    break;

                                case "valid":
                                    if (long.TryParse(value, out long a2Valid))
                                    {
                                        dataSet.TelegramData.A2_Valid = a2Valid;
                                    }
                                    break;
                            }
                            break;

                        case "position":
                            switch (field)
                            {
                                case "areaNumber":
                                    if (long.TryParse(value, out long areaNumber))
                                    {
                                        dataSet.PositionData.AreaNumber = areaNumber;
                                    }
                                    break;

                                case "latitude":
                                    if (double.TryParse(value, out double latitude))
                                    {
                                        dataSet.PositionData.Latitude = latitude;
                                    }
                                    break;

                                case "longitude":
                                    if (double.TryParse(value, out double longitude))
                                    {
                                        dataSet.PositionData.Longitude = longitude;
                                    }
                                    break;

                                case "position":
                                    if (long.TryParse(value, out long position))
                                    {
                                        dataSet.PositionData.Position = position;
                                    }
                                    break;

                                case "positionNoLeap":
                                    if (long.TryParse(value, out long positionNoLeap))
                                    {
                                        dataSet.PositionData.PositionNoLeap = positionNoLeap;
                                    }
                                    break;
                            }
                            break;

                        case "a2_rssi":
                            switch (field)
                            {
                                case "signalStrength":
                                    if (double.TryParse(value, out double signalStrength))
                                    {
                                        dataSet.RssiData.Value = signalStrength;
                                        dataSet.RssiData.Timestamp = timestamp;
                                    }
                                    break;

                                case "anomaly":
                                    if (double.TryParse(value, out double anomalyIndicator))
                                    {
                                        dataSet.RssiData.HasAnomaly = anomalyIndicator < 0;
                                    }
                                    break;
                            }
                            break;
                    }
                }

                result.Add(dataSet);
            }

            return result;
        }

        private void InitializeHttpClient()
        {
        }

        private void InitializeInfluxDbClient()
        {
            SetCulture();

            _settings = new InfluxDbSettings()
            {
                Bucket = Environment.GetEnvironmentVariable("INFLUXDB_BUCKET"),
                Organization = Environment.GetEnvironmentVariable("INFLUXDB_ORGANIZATION"),
                Token = Environment.GetEnvironmentVariable("INFLUXDB_TOKEN"),
                Url = Environment.GetEnvironmentVariable("INFLUXDB_URL")
            };

            _client = InfluxDBClientFactory.Create(_settings.Url, _settings.Token.ToCharArray());
        }

        private void SetCulture()
        {
            var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            Thread.CurrentThread.CurrentCulture = customCulture;
        }
    }
}