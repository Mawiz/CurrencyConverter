using CurrencyConverter.Business.Contracts;
using CurrencyConverter.Business.Dto;
using CurrencyConverter.Business.Provider;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Serilog;
using System.Diagnostics;
using System.Net;

namespace CurrencyConverter.Business.Core
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        private readonly IExchangeRateProviderFactory _providerFactory;
        private readonly IExchangeRateProvider _provider;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

        private static readonly HashSet<string> ExcludedCurrencies = new() { "TRY", "PLN", "THB", "MXN" };

        public ExchangeRateService(
            IExchangeRateProviderFactory providerFactory,
            IMemoryCache cache,
            ILogger logger)
        {
            _providerFactory = providerFactory;
            _provider = _providerFactory.GetProvider("frankfurter"); // default provider
            _cache = cache;
            _logger = logger;

            _retryPolicy = Policy.Handle<HttpRequestException>()
                                 .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                    (exception, timeSpan, retryCount, context) =>
                                    {
                                        _logger.Warning("Retry {RetryCount} due to {Exception}", retryCount, exception.Message);
                                    });

            _circuitBreaker = Policy.Handle<HttpRequestException>()
                                    .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30),
                                    onBreak: (ex, ts) => _logger.Error("Circuit breaker opened for {Duration} due to {Exception}", ts, ex.Message),
                                    onReset: () => _logger.Information("Circuit breaker reset."));
        }

        public async Task<ResponseDto<LatestRatesResponseDto>> GetLatestRatesAsync(LatestRatesRequestDto request)
        {
            var response = new ResponseDto<LatestRatesResponseDto>();
            var stopwatch = Stopwatch.StartNew();

            _logger.Information("Fetching latest rates for base currency: {BaseCurrency}", request.BaseCurrency);

            try
            {
                if (ExcludedCurrencies.Contains(request.BaseCurrency.ToUpper()))
                {
                    response.AddError($"Base currency {request.BaseCurrency} is not allowed.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                string cacheKey = $"LatestRates_{request.BaseCurrency}";
                if (_cache.TryGetValue(cacheKey, out Dictionary<string, decimal> cachedRates))
                {
                    response.Result = new LatestRatesResponseDto
                    {
                        BaseCurrency = request.BaseCurrency,
                        Rates = cachedRates
                    };
                    response.Message = "Rates retrieved from cache.";
                    return response;
                }

                var apiResponse = await _retryPolicy.WrapAsync(_circuitBreaker).ExecuteAsync(() =>
                    _provider.GetLatestRatesAsync(request.BaseCurrency));

                if (apiResponse?.Rates == null)
                {
                    response.AddError("Failed to fetch exchange rates from provider.");
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }

                foreach (var currency in ExcludedCurrencies)
                    apiResponse.Rates.Remove(currency);
                
                apiResponse.BaseCurrency = request.BaseCurrency;

                _cache.Set(cacheKey, apiResponse.Rates, TimeSpan.FromHours(1));

                response.Result = apiResponse;
                response.Message = $"Latest exchange rates retrieved successfully for {request.BaseCurrency}.";
            }
            catch (Exception ex)
            {
                response.AddError("Error fetching latest exchange rates: " + ex.Message);
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Exception = ex.ToString();
                _logger.Error(ex, "Error fetching latest rates for {BaseCurrency}", request.BaseCurrency);
            }
            finally
            {
                stopwatch.Stop();
                _logger.Information("Completed fetching latest rates for {BaseCurrency} in {Duration}ms", request.BaseCurrency, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }

        public async Task<ResponseDto<HistoricalRatesResponseDto>> GetHistoricalRatesAsync(HistoricalRatesRequestDto request)
        {
            var response = new ResponseDto<HistoricalRatesResponseDto>();
            var stopwatch = Stopwatch.StartNew();

            _logger.Information("Fetching historical rates for base currency: {BaseCurrency} from {StartDate} to {EndDate}",
                request.BaseCurrency, request.StartDate, request.EndDate);

            try
            {
                if (ExcludedCurrencies.Contains(request.BaseCurrency.ToUpper()))
                {
                    response.AddError($"Base currency {request.BaseCurrency} is not allowed.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                string cacheKey = $"HistoricalRates_{request.BaseCurrency}_{request.StartDate:yyyyMMdd}_{request.EndDate:yyyyMMdd}";

                // Try get full rates from cache
                if (!_cache.TryGetValue(cacheKey, out Dictionary<string, Dictionary<string, decimal>> cachedRates))
                {
                    // Fetch from API if not in cache
                    var apiResponse = await _retryPolicy.WrapAsync(_circuitBreaker).ExecuteAsync(() =>
                        _provider.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate));

                    if (apiResponse?.Rates == null)
                    {
                        response.AddError("Failed to fetch historical exchange rates.");
                        response.StatusCode = HttpStatusCode.InternalServerError;
                        return response;
                    }

                    // Remove excluded currencies
                    foreach (var dateRates in apiResponse.Rates)
                        foreach (var currency in ExcludedCurrencies)
                            dateRates.Value.Remove(currency);

                    cachedRates = apiResponse.Rates;

                    // Cache the full rates dictionary
                    _cache.Set(cacheKey, cachedRates, TimeSpan.FromMinutes(60));
                }

                // Pagination
                var queryable = cachedRates.OrderBy(x => x.Key).AsQueryable();
                queryable = request.OrderBy.ToLower() switch
                {
                    "date" => request.Ascending ? queryable.OrderBy(x => x.Key) : queryable.OrderByDescending(x => x.Key),
                    _ => queryable
                };

                var paginated = queryable
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // Prepare response
                response.Result = new HistoricalRatesResponseDto
                {
                    BaseCurrency = request.BaseCurrency,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Rates = paginated
                };

                response.Message = $"Historical exchange rates retrieved successfully for {request.BaseCurrency}.";
            }
            catch (Exception ex)
            {
                response.AddError("Error fetching historical exchange rates: " + ex.Message);
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Exception = ex.ToString();
                _logger.Error(ex, "Error fetching historical rates for {BaseCurrency}", request.BaseCurrency);
            }
            finally
            {
                stopwatch.Stop();
                _logger.Information("Completed fetching historical rates for {BaseCurrency} in {Duration}ms",
                    request.BaseCurrency, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }

        public async Task<ResponseDto<CurrencyConversionResponseDto>> ConvertAsync(CurrencyConversionRequestDto request)
        {
            var response = new ResponseDto<CurrencyConversionResponseDto>();
            var stopwatch = Stopwatch.StartNew();
            var today = DateTime.UtcNow.Date; // Use today's date for cache key

            _logger.Information("Starting currency conversion: {Amount} {FromCurrency} -> {ToCurrency}",
                request.Amount, request.FromCurrency, request.ToCurrency);

            try
            {
                // Validate excluded currencies
                if (ExcludedCurrencies.Contains(request.FromCurrency.ToUpper()) ||
                    ExcludedCurrencies.Contains(request.ToCurrency.ToUpper()))
                {
                    response.AddError($"Conversion involving {request.FromCurrency} or {request.ToCurrency} is not allowed.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                // Cache key uses from/to currency + today
                string cacheKey = $"Conversion_{request.FromCurrency}_{request.ToCurrency}_{today:yyyyMMdd}";

                if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
                {
                    response.Result = new CurrencyConversionResponseDto
                    {
                        FromCurrency = request.FromCurrency,
                        ToCurrency = request.ToCurrency,
                        OriginalAmount = request.Amount,
                        ConvertedAmount = request.Amount * cachedRate
                    };
                    response.Message = "Conversion result retrieved from cache.";
                    return response;
                }

                // Fetch latest rates for conversion via provider
                var latestRates = await _retryPolicy.WrapAsync(_circuitBreaker).ExecuteAsync(() =>
                    _provider.GetLatestRatesAsync(request.FromCurrency));

                if (latestRates?.Rates == null || !latestRates.Rates.ContainsKey(request.ToCurrency.ToUpper()))
                {
                    response.AddError($"Unable to convert from {request.FromCurrency} to {request.ToCurrency}.");
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }

                var rate = latestRates.Rates[request.ToCurrency.ToUpper()];

                // Cache the rate itself (not the converted amount) for today
                _cache.Set(cacheKey, rate, TimeSpan.FromHours(24));

                var convertedAmount = request.Amount * rate;

                response.Result = new CurrencyConversionResponseDto
                {
                    FromCurrency = request.FromCurrency,
                    ToCurrency = request.ToCurrency,
                    OriginalAmount = request.Amount,
                    ConvertedAmount = convertedAmount
                };

                response.Message = $"Successfully converted {request.Amount} {request.FromCurrency} to {convertedAmount} {request.ToCurrency}.";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.AddError("Error during currency conversion: " + ex.Message);
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Exception = ex.ToString();
                _logger.Error(ex, "Error converting {FromCurrency} to {ToCurrency}", request.FromCurrency, request.ToCurrency);
            }
            finally
            {
                stopwatch.Stop();
                _logger.Information("Completed currency conversion for {FromCurrency} -> {ToCurrency} in {Duration}ms",
                    request.FromCurrency, request.ToCurrency, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }

    }
}
