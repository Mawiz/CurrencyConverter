using CurrencyConverter.Business.Contracts;
using CurrencyConverter.Business.Dto;
using CurrencyConverter.Common.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CurrencyConverter.Business.Core
{
    public class AuthService : IAuthService
    {
        private readonly JWTSettings _jwtSettings;

        // Example in-memory users, replace with DB
        private readonly Dictionary<string, (string Password, string[] Roles)> _users = new()
        {
            { "admin", ("admin123", new[] { "Admin" }) },
            { "user", ("user123", new[] { "User" }) }
        };

        public AuthService(IOptions<JWTSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public Task<ResponseDto<LoginResponseDto>> AuthenticateAsync(LoginRequestDto login)
        {
            var response = new ResponseDto<LoginResponseDto>();

            if (login == null || string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
            {
                response.AddError("Username or password is missing.");
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return Task.FromResult(response);
            }

            if (!_users.TryGetValue(login.Username.ToLower(), out var userInfo) || userInfo.Password != login.Password)
            {
                response.AddError("Invalid username or password.");
                response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                return Task.FromResult(response);
            }

            var token = GenerateJwtToken(login.Username, userInfo.Roles);
            response.Result = new LoginResponseDto { Token = token };
            response.StatusCode = System.Net.HttpStatusCode.OK;
            response.Message = "Login successful.";

            return Task.FromResult(response);
        }

        private string GenerateJwtToken(string username, string[] roles)
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenLifespan),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}