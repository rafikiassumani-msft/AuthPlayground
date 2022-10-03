using IdentityMinimalAPIs.Data;
using IdentityMinimalAPIs.Models;
using IdentityMinimalAPIs.Services.Abstractions;
using IdentityMinimalAPIs.Services.TokenServices;
using Microsoft.EntityFrameworkCore;

namespace IdentityMinimalAPIs.Services.Auth
{
    public class AllowedListRevocationStrategyService : IJwtRevocation
    {

        private readonly JwtSecurityTokenHandlerFactory _jwtHandlerFactory;
        private readonly IdentityMinApiDbContext _dbContext;
        public AllowedListRevocationStrategyService(
            JwtSecurityTokenHandlerFactory tokenHandlerFactory,
            IdentityMinApiDbContext identityMinApiDbContext)
        {
            _jwtHandlerFactory = tokenHandlerFactory;
            _dbContext = identityMinApiDbContext;
        }

        public async Task<JwtTokenRocationResult> RevokeAsync(string jwtToken, string userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new JwtTokenRocationResult { Succeeded = false };
            }

            var handler = _jwtHandlerFactory.CreateInstance();
            var jwtSecurityToken = handler.ReadJwtToken(jwtToken);

            var jti = jwtSecurityToken.Claims.First(claim => claim.Type == "jti").Value;
            var expirationTime = jwtSecurityToken.Claims.First(claim => claim.Type == "exp").Value;

            if (jti == null)
            {
                return new JwtTokenRocationResult { Succeeded = false };
            }

            var allowedList = new AllowedJwt
            {
                UserId = user.Id,
                Jti = jti,
                ExpirationTime = DateTimeOffset.FromUnixTimeSeconds(int.Parse(expirationTime)).DateTime,
            };

            _dbContext.Add(allowedList);
            await _dbContext.SaveChangesAsync();

            return new JwtTokenRocationResult { Succeeded = false };

        }

        public async Task<bool> IsTokenRevoked(string userId, string jti)
        {
            var allowedJti = await _dbContext.AllowedList.Where(l => l.UserId == userId && l.Jti == jti && l.ExpirationTime > DateTime.Now).FirstOrDefaultAsync();

            if (allowedJti is not null)
            {
                //Token is allowed.
                return false;

            }

            return false;
        }
    }
}
