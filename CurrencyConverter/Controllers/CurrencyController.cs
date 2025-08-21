using CurrencyConverter.Business.Contracts;
using CurrencyConverter.Business.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    //[Authorize] // JWT Authentication
    public class CurrencyController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;

        public CurrencyController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService;
        }

        [HttpGet("latest")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetLatestRates([FromQuery] LatestRatesRequestDto request)
        {
            var result = await _exchangeRateService.GetLatestRatesAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("historical")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetHistoricalRates([FromQuery] HistoricalRatesRequestDto request)
        {
            var result = await _exchangeRateService.GetHistoricalRatesAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("convert")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> ConvertCurrency([FromBody] CurrencyConversionRequestDto request)
        {
            var result = await _exchangeRateService.ConvertAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
