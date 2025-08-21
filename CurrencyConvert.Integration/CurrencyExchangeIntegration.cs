using CurrencyConverter.Business.Dto;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;

namespace CurrencyConvert.Integration
{
    [TestFixture]
    public class CurrencyExchangeIntegration
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task GetLatestRates_ShouldReturnRates()
        {
            // Arrange
            var request = new LatestRatesRequestDto { BaseCurrency = "EUR" };

            // Act
            var response = await _client.GetAsync($"/api/v1/currency/latest?BaseCurrency={request.BaseCurrency}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<LatestRatesResponseDto>();
            content.Should().NotBeNull();
            content.Rates.Should().ContainKey("USD");
        }

        [Test]
        public async Task GetHistoricalRates_ShouldReturnPaginatedRates()
        {
            // Arrange
            var request = new HistoricalRatesRequestDto
            {
                BaseCurrency = "EUR",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 1, 3),
                PageNumber = 1,
                PageSize = 2
            };

            var query = $"?BaseCurrency={request.BaseCurrency}&StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}&PageNumber={request.PageNumber}&PageSize={request.PageSize}";

            // Act
            var response = await _client.GetAsync($"/api/v1/currency/historical{query}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<HistoricalRatesResponseDto>();
            content.Should().NotBeNull();
            content.Rates.Count.Should().BeLessThanOrEqualTo(request.PageSize);
        }

        [Test]
        public async Task ConvertCurrency_ShouldReturnConvertedAmount()
        {
            // Arrange
            var request = new CurrencyConversionRequestDto
            {
                FromCurrency = "EUR",
                ToCurrency = "USD",
                Amount = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/currency/convert", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<CurrencyConversionResponseDto>();
            content.Should().NotBeNull();
            content.ConvertedAmount.Should().BeGreaterThan(0);
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
