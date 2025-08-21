using CurrencyConverter.Business.Dto;

namespace CurrencyConverter.Business.Contracts
{
    public interface IExchangeRateService
    {
        /// <summary>
        /// Fetch latest exchange rates for a specific base currency
        /// </summary>
        Task<ResponseDto<LatestRatesResponseDto>> GetLatestRatesAsync(LatestRatesRequestDto request);

        /// <summary>
        /// Fetch historical exchange rates for a given period with pagination
        /// </summary>
        Task<ResponseDto<HistoricalRatesResponseDto>> GetHistoricalRatesAsync(HistoricalRatesRequestDto request);

        /// <summary>
        /// Convert an amount from one currency to another using latest exchange rates
        /// </summary>
        Task<ResponseDto<CurrencyConversionResponseDto>> ConvertAsync(CurrencyConversionRequestDto request);

    }
}
