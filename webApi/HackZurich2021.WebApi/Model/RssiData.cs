using System;

namespace HackZurich2021.WebApi.Model
{
    public class RssiData
    {
        public enum RssiDataQuality
        {
            Undefined,
            Weak,
            Fair,
            Good,
            Excellent
        }

        public bool HasAnomaly { get; set; }
        public RssiDataQuality Quality { get => EvaluateQuality(); }
        public DateTime? Timestamp { get; set; }
        public double Value { get; set; }

        private RssiDataQuality EvaluateQuality()
        {
            if (double.IsNaN(Value)
                || Value > 2.9
                || Value < 0.9)
            {
                return RssiDataQuality.Undefined;
            }
            else if (Value >= 0.9 && Value < 1.2)
            {
                return RssiDataQuality.Weak;
            }
            else if (Value >= 1.2 && Value < 1.6)
            {
                return RssiDataQuality.Fair;
            }
            else if (Value >= 1.6 && Value < 2.0)
            {
                return RssiDataQuality.Good;
            }
            else if (Value >= 2.0 && Value < 2.9)
            {
                return RssiDataQuality.Excellent;
            }

            return RssiDataQuality.Undefined;
        }
    }
}