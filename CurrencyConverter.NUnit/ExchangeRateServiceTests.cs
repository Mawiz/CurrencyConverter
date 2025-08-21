using CurrencyConverter.Business.Core;
using CurrencyConverter.Business.Dto;
using CurrencyConverter.Business.Provider;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Serilog;

namespace CurrencyConverter.Business.Test.UnitTest
{

    [TestFixture]
    public class ExchangeRateServiceTests
    {
        private Mock<IExchangeRateProviderFactory> _providerFactoryMock;
        private Mock<IExchangeRateProvider> _providerMock;
        private Mock<ILogger> _loggerMock;
        private IMemoryCache _cache;
        private ExchangeRateService _service;

        [SetUp]
        public void Setup()
        {
            _providerFactoryMock = new Mock<IExchangeRateProviderFactory>();
            _providerMock = new Mock<IExchangeRateProvider>();
            _loggerMock = new Mock<ILogger>();
            _cache = new MemoryCache(new MemoryCacheOptions());

            _providerFactoryMock.Setup(x => x.GetProvider(It.IsAny<string>()))
                                .Returns(_providerMock.Object);

            _service = new ExchangeRateService(_providerFactoryMock.Object, _cache, _loggerMock.Object);
        }

        [Test]
        public async Task GetLatestRatesAsync_ReturnsRates_WhenProviderReturnsData()
        {
            var request = new LatestRatesRequestDto { BaseCurrency = "EUR" };
            var responseDto = new LatestRatesResponseDto
            {
                BaseCurrency = "EUR",
                Rates = new Dictionary<string, decimal> { { "USD", 1.1M } }
            };

            _providerMock.Setup(p => p.GetLatestRatesAsync("EUR"))
                         .ReturnsAsync(responseDto);

            var result = await _service.GetLatestRatesAsync(request);

            result.Should().NotBeNull();
            result.Result.Rates.Should().ContainKey("USD");
            result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test]
        public async Task GetHistoricalRatesAsync_ReturnsPaginatedRates()
        {
            var request = new HistoricalRatesRequestDto
            {
                BaseCurrency = "EUR",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 1, 3),
                PageNumber = 1,
                PageSize = 2
            };

            var apiRates = new Dictionary<string, Dictionary<string, decimal>>
            {
                { "2020-01-01", new Dictionary<string, decimal>{{"USD", 1.1M}} },
                { "2020-01-02", new Dictionary<string, decimal>{{"USD", 1.2M}} },
                { "2020-01-03", new Dictionary<string, decimal>{{"USD", 1.3M}} }
            };

            _providerMock.Setup(p => p.GetHistoricalRatesAsync("EUR", request.StartDate, request.EndDate))
                         .ReturnsAsync(new HistoricalRatesResponseDto { Rates = apiRates });

            var result = await _service.GetHistoricalRatesAsync(request);

            result.Result.Rates.Count.Should().Be(2); // Pagination applied
        }

        [Test]
        public async Task ConvertAsync_ReturnsConvertedAmount()
        {
            var request = new CurrencyConversionRequestDto
            {
                FromCurrency = "EUR",
                ToCurrency = "USD",
                Amount = 10
            };

            var latestRates = new LatestRatesResponseDto
            {
                BaseCurrency = "EUR",
                Rates = new Dictionary<string, decimal> { { "USD", 1.5M } }
            };

            _providerMock.Setup(p => p.GetLatestRatesAsync("EUR"))
                         .ReturnsAsync(latestRates);

            var result = await _service.ConvertAsync(request);

            result.Result.ConvertedAmount.Should().Be(15M);
        }

        [TearDown]
        public void TearDown()
        {
            _cache.Dispose();
        }
    }

}
