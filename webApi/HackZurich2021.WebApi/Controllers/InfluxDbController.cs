using HackZurich2021.WebApi.Model;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace HackZurich2021.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InfluxDbController : ControllerBase
    {
        public InfluxDbController(ILogger<InfluxDbController> logger)
        {
            _logger = logger;

            InitializeClient();
        }

        [HttpGet]
        [Route(nameof(GetLastRssiData))]
        public IEnumerable<RssiData> GetLastRssiData(int seconds)
        {
            var query = $"from(bucket: \"{_settings.Bucket}\") |> range(start: -{seconds}s) |> filter(fn: (r) => r[\"category\"] == \"rssi\")";
            var raw = _client.GetQueryApi().QueryAsync(query, _settings.Organization).Result;

            return GetRssiDataFromFluxTables(raw);
        }

        [HttpGet]
        [Route(nameof(GetRssiDataByTimeRange))]
        public IEnumerable<RssiData> GetRssiDataByTimeRange(DateTime start, DateTime end)
        {
            var query = $"from(bucket: \"{_settings.Bucket}\") |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ}) |> filter(fn: (r) => r[\"category\"] == \"rssi\")";
            var raw = _client.GetQueryApi().QueryAsync(query, _settings.Organization).Result;

            return GetRssiDataFromFluxTables(raw);
        }

        private readonly ILogger<InfluxDbController> _logger;

        private InfluxDBClient _client;

        private InfluxDbSettings _settings;

        private List<RssiData> GetRssiDataFromFluxTables(List<FluxTable> fluxTables)
        {
            var result = new List<RssiData>();
            for (int i = 0; i < fluxTables.Max(t => t.Records.Count); i++)
            {
                var rssiData = new RssiData();

                foreach (var table in fluxTables)
                {
                    if (table.Records.Count <= i)
                    {
                        continue;
                    }

                    var record = table.Records[i];
                    var field = record.GetField();
                    var value = record.GetValue().ToString();
                    var timestamp = record.GetTimeInDateTime();

                    switch (field)
                    {
                        case "signalStrength":
                            if (double.TryParse(value, out double signalStrength))
                            {
                                rssiData.Value = signalStrength;
                                rssiData.Timestamp = timestamp;
                            }
                            break;

                        case "anomaly":
                            if (double.TryParse(value, out double anomalyIndicator))
                            {
                                rssiData.HasAnomaly = anomalyIndicator < 0;
                            }
                            break;
                    }
                }

                result.Add(rssiData);
            }

            return result;
        }

        private void InitializeClient()
        {
            SetCulture();

            using (var r = new StreamReader(@".\influxDbSettings.json"))
            {
                _settings = JsonSerializer.Deserialize<InfluxDbSettings>(r.ReadToEnd());
            }

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