using IdentityMinimalAPIs.Data;
using IdentityMinimalAPIs.Services.Abstractions;
using IdentityMinimalAPIs.Services.TokenServices;
using Microsoft.EntityFrameworkCore;

namespace IdentityMinimalAPIs.Services.Auth
{
    public class JtiMatcherRevocationStrategyService : IJwtRevocation
    {
        private readonly JwtSecurityTokenHandlerFactory _jwtHandlerFactory;
        private readonly IdentityMinApiDbContext _dbContext;
        public JtiMatcherRevocationStrategyService(
            JwtSecurityTokenHandlerFactory tokenHandlerFactory,
            IdentityMinApiDbContext identityMinApiDbContext)
        {
            _jwtHandlerFactory = tokenHandlerFactory;
            _dbContext = identityMinApiDbContext;
        }

        public async Task<JwtTokenRocationResult> RevokeAsync(string token, string userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new JwtTokenRocationResult { Succeeded = false };
            }

            var tokenHandler = _jwtHandlerFactory.CreateInstance();
            var jwtSecurityToken = tokenHandler.ReadJwtToken(token);

            var jti = jwtSecurityToken.Claims.First(claim => claim.Type == "jti").Value;

            if (jti is not null && jti == user.Jti)
            {
                //Set the jti to null and save changes
                user.Jti = null;
                await _dbContext.SaveChangesAsync();

                return new JwtTokenRocationResult { Succeeded = true };
            }

            return new JwtTokenRocationResult { Succeeded = false };
        }

        public async Task<bool> IsTokenRevoked(string userId, string jti)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || string.IsNullOrEmpty(jti))
            {
                return true;
            }

            if (user.Jti == null || user.Jti != jti)
            {
                return true;
            }

            return false;

        }
    }

}
