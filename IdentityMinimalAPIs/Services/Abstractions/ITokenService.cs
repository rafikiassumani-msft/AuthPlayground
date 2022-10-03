using IdentityMinimalAPIs.DTOs;
using IdentityMinimalAPIs.Models;
using System.Security.Claims;

namespace SharedIdentityServices.Abstractions
{
    public interface ITokenService
    {
        Task<string> GetJwtTokenAsync(User user);
        Task<TokenDTO> GetTokensAsync(User user);
        Task<string> GetRefreshToken(User user);
        ClaimsPrincipal? ValidateAccessToken(string token);
        Task<bool> ValidateRefreshToken(string token);
        Task RevokeRefreshTokenAsync(User user, string refreshToken);
    }
}
