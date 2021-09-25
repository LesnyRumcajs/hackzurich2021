namespace HackZurich2021.WebApi.Model
{
    public class TelegramData
    {
        public long A1_Total { get; set; }
        public long A1_Valid { get; set; }
        public long A2_Total { get; set; }
        public long A2_Valid { get; set; }

        public string Print()
        {
            return $"A1 Total: {A1_Total} | A1 Valid: {A1_Valid} | A2 Total: {A2_Total} | A2 Valid: {A2_Valid}";
        }
    }
}