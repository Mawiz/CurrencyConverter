using CurrencyConverter.Business.Dto;

namespace CurrencyConverter.Business.Contracts
{
    public interface IAuthService
    {
        Task<ResponseDto<LoginResponseDto>> AuthenticateAsync(LoginRequestDto login);
    }
}
