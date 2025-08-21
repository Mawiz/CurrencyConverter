namespace CurrencyConverter.Business.Dto
{
    // Latest Rates
    public class LatestRatesRequestDto
    {
        public string BaseCurrency { get; set; } = "EUR";
    }

    public class LatestRatesResponseDto
    {
        public string BaseCurrency { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }

}
