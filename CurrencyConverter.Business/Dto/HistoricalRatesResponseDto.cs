using System.Text.Json.Serialization;

namespace CurrencyConverter.Business.Dto
{
    public class HistoricalRatesRequestDto
    {
        public string BaseCurrency { get; set; } = "EUR";

        public DateTime StartDate { get; set; } = new DateTime(2020, 1, 1);
        public DateTime EndDate { get; set; } = new DateTime(2020, 1, 31);
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string OrderBy { get; set; } = "Date";
        public bool Ascending { get; set; } = true;
    }

    public class HistoricalRatesResponseDto
    {
        [JsonPropertyName("base")]
        public string BaseCurrency { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = new();
    }
}
