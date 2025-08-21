namespace CurrencyConverter.Business.Dto
{
    public class ExchangeRateResponseDto
    {
        public string Base { get; set; }       // "EUR"
        public DateTime Date { get; set; }     // "2025-08-19"
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
