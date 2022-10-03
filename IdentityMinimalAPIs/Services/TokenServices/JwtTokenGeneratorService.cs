using IdentityMinimalAPIs.Data;
using IdentityMinimalAPIs.DTOs;
using IdentityMinimalAPIs.Models;
using IdentityMinimalAPIs.Services.ConfigExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedIdentityServices.Abstractions;
using System.Security.Claims;
using System.Text;

namespace IdentityMinimalAPIs.Services.TokenServices
{
    public class JwtTokenGeneratorService : ITokenService
    {
        private readonly JwtSecurityTokenHandlerFactory _jwtHandlerFactory;
        private readonly ILogger<JwtTokenGeneratorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly JwtRevocationStrategyOptions _revocationOptions;
        private readonly IdentityMinApiDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;

        public JwtTokenGeneratorService(IConfiguration configuration,
                                JwtSecurityTokenHandlerFactory jwtHandlerFactory,
                                UserManager<User> userManager,
                                ILogger<JwtTokenGeneratorService> logger,
                                IOptions<JwtRevocationStrategyOptions> revocationOptions,
                                IOptions<JwtSettings> jwtSettingOtions,
                                IdentityMinApiDbContext dbContext)
        {
            _configuration = configuration;
            _jwtHandlerFactory = jwtHandlerFactory;
            _userManager = userManager;
            _logger = logger;
            _jwtSettings = jwtSettingOtions.Value;
            _revocationOptions = revocationOptions.Value;
            _dbContext = dbContext;
        }
        public async Task<string> GetJwtTokenAsync(User user)
        {
            var jti = Guid.NewGuid().ToString();
            var tokenExpiryTime = DateTime.UtcNow.AddHours(1);
            var token = await GetTokenAsync(user, jti, tokenExpiryTime);

            if (_revocationOptions.StrategyName == JwtRevocationStrategyConstants.JtiMatchter)
            {
                var currentUser = await _dbContext.Users.FindAsync(user.Id);

                if (currentUser is not null)
                {
                    currentUser.Jti = jti;
                    await _dbContext.SaveChangesAsync();
                }
            }
            else if (_revocationOptions.StrategyName == JwtRevocationStrategyConstants.AllowList)
            {
                var allowedJwt = new AllowedJwt
                {
                    Jti = jti,
                    UserId = user.Id,
                    ExpirationTime = tokenExpiryTime,
                };

                _dbContext.Add(allowedJwt);
                await _dbContext.SaveChangesAsync();

            }

            return token;
        }

        public async Task<string> GetRefreshToken(User user)
        {
            var uuid = Guid.NewGuid().ToString();

            var refreshToken = new RefreshToken
            {
                Token = uuid,
                UserId = user.Id,
                Expiry = DateTime.Now.AddHours(24), //Default to 24 hours
                IsRevoked = false,
            };

            _dbContext.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return uuid;
        }

        public ClaimsPrincipal? ValidateAccessToken(string jwtToken)
        {
            try
            {
                return _jwtHandlerFactory.CreateInstance().ValidateToken(jwtToken, new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(ReadKeyAsBytes(_jwtSettings.TokenSecretKey))
                }, out var validatedJwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate jwt token");
            }

            return null;
        }

        public async Task<TokenDTO> GetTokensAsync(User user)
        {
            var jwtToken = await GetJwtTokenAsync(user);
            var refreshToken = await GetRefreshToken(user);
            //Get Refresh token

            return new TokenDTO
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken,
            };
        }

        public async Task<bool> ValidateRefreshToken(string token)
        {
            var refreshToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rf => rf.Token == token
                                    && rf.IsRevoked == false
                                    && DateTime.Now < rf.Expiry);

            if (refreshToken is not null)
            {
                return true;
            }
            return false;
        }

        public async Task RevokeRefreshTokenAsync(User user, string refreshToken)
        {
            var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (token is not null)
            {
                token.IsRevoked = true;
                token.Expiry = DateTime.Now;
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task<string> GetTokenAsync(User user, string jti, DateTime tokenExpiryTime)
        {

            var handler = _jwtHandlerFactory.CreateInstance();
            var signingKey = _jwtSettings.TokenSecretKey;
            var claims = await GetAccessTokenClaims(user, jti);

            var jwtToken = handler.CreateToken(GetSecurityTokenDescriptor(user, signingKey, claims, tokenExpiryTime));

            return handler.WriteToken(jwtToken);
        }

        private static byte[] ReadKeyAsBytes(string key)
        {
            var ascii = Encoding.ASCII;
            return ascii.GetBytes(key);
        }

        private SecurityTokenDescriptor GetSecurityTokenDescriptor(User user, string signingKey, IDictionary<string, object> claims, DateTime tokenExpirationTime)
        {

            return new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Subject = new ClaimsIdentity(new[] { new Claim("userId", user.Id) }),
                Expires = tokenExpirationTime,
                IssuedAt = DateTime.UtcNow,
                Claims = claims,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(ReadKeyAsBytes(signingKey)), SecurityAlgorithms.HmacSha256Signature)
            };
        }

        private async Task<IDictionary<string, object>> GetAccessTokenClaims(User user, string jti)
        {

            var customClaims = await _userManager.GetClaimsAsync(user);
            var customClaimsMap = customClaims.ToDictionary(keySelector: claim => claim.Type, elementSelector: claim => claim.Value);

            var claims = new Dictionary<string, object>
            {
                { nameof(jti), jti },
                { nameof(user.FirstName), user.FirstName },
                { nameof(user.LastName), user.LastName },
                { nameof(user.Email), user.Email! },
                { nameof(user.UserName), user.UserName! },
                { nameof(user.Id), user.Id },
                { nameof(user.Preferred2fa), user.Preferred2fa! },
                { "customClaims", customClaimsMap }
            };

            return claims;
        }

    }
}
