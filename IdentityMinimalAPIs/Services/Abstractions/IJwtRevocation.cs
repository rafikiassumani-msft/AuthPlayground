using IdentityMinimalAPIs.Models;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityMinimalAPIs.Services.Abstractions
{
    public interface IJwtRevocation
    {
        Task<JwtTokenRocationResult> RevokeAsync(string jwtToken, string userId);
        Task<bool> IsTokenRevoked(string userId, string jti);    

    }

    public class JwtTokenRocationResult
    {
        public bool Succeeded { get; set; }    
    }
}
