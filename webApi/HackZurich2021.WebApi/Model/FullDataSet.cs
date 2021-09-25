using System;

namespace HackZurich2021.WebApi.Model
{
    public class FullDataSet
    {
        public PositionData PositionData { get; set; } = new PositionData();
        public RssiData RssiData { get; set; } = new RssiData();
        public TelegramData TelegramData { get; set; } = new TelegramData();
        public DateTime? Timestamp { get; set; }
    }
}