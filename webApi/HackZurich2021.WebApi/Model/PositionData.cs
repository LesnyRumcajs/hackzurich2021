namespace HackZurich2021.WebApi.Model
{
    public class PositionData
    {
        public long AreaNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long Position { get; set; }
        public long PositionNoLeap { get; set; }
    }
}