using CurrencyConverter.Business.Dto;
using System.Net.Http.Json;

namespace CurrencyConverter.Business.Provider
{
    public class FrankfurterProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;

        public string Name => "Frankfurter";

        public FrankfurterProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.frankfurter.app/");
        }

        public async Task<LatestRatesResponseDto> GetLatestRatesAsync(string baseCurrency)
        {
            var response = await _httpClient.GetFromJsonAsync<LatestRatesResponseDto>($"latest?from={baseCurrency}");
            return response;
        }

        public async Task<HistoricalRatesResponseDto> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate)
        {
            var response = await _httpClient.GetFromJsonAsync<HistoricalRatesResponseDto>(
                $"{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?from={baseCurrency}");
            return response;
        }
    }
}
