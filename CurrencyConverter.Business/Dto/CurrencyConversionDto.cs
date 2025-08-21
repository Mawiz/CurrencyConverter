namespace CurrencyConverter.Business.Dto
{
    public class CurrencyConversionRequestDto
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
    }

    public class CurrencyConversionResponseDto
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
    }

}
