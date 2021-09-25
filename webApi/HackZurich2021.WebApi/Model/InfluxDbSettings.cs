namespace HackZurich2021.WebApi.Model
{
    public class InfluxDbSettings
    {
        public string Bucket { get; set; }
        public string Organization { get; set; }
        public string Token { get; set; }
        public string Url { get; set; }
    }
}