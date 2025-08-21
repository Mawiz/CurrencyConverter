using CurrencyConverter.Business.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Business.Provider
{
    public interface IExchangeRateProvider
    {
        string Name { get; }
        Task<LatestRatesResponseDto> GetLatestRatesAsync(string baseCurrency);
        Task<HistoricalRatesResponseDto> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate);
    }
    public interface IExchangeRateProviderFactory
    {
        IExchangeRateProvider GetProvider(string providerName);
    }
    public class FrankfurterProviderFactory : IExchangeRateProviderFactory
    {
        private readonly Dictionary<string, IExchangeRateProvider> _providers;

        public FrankfurterProviderFactory(FrankfurterProvider frankfurterProvider)
        {
            _providers = new Dictionary<string, IExchangeRateProvider>(StringComparer.OrdinalIgnoreCase)
        {
            { "frankfurter", frankfurterProvider }
        };
        }

        public IExchangeRateProvider GetProvider(string providerName)
        {
            if (_providers.TryGetValue(providerName, out var provider))
                return provider;

            throw new NotSupportedException($"Provider '{providerName}' is not supported.");
        }
    }
}
