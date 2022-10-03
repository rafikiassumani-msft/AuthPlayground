using IdentityMinimalAPIs.Data;
using IdentityMinimalAPIs.Services.Abstractions;
using IdentityMinimalAPIs.Services.ConfigExtensions;
using IdentityMinimalAPIs.Services.TokenServices;
using Microsoft.Extensions.Options;

namespace IdentityMinimalAPIs.Services.Auth
{
    public class JwtRevocationStrategyFactory : IJwtRevocationStrategyFactory
    {
        private readonly JwtRevocationStrategyOptions _jwtRevocationStrategyOptions;
        private readonly ILogger<JwtRevocationStrategyFactory> _logger;
        private readonly JwtSecurityTokenHandlerFactory _jwtHandlerFactory;
        private readonly IdentityMinApiDbContext _dbContext;

        public JwtRevocationStrategyFactory(
            IOptions<JwtRevocationStrategyOptions> jwtRevocationStrategyOptions,
            ILogger<JwtRevocationStrategyFactory> logger,
            JwtSecurityTokenHandlerFactory jwtHandlerFactory,
            IdentityMinApiDbContext dbContext)
        {
            _jwtRevocationStrategyOptions = jwtRevocationStrategyOptions.Value;
            _logger = logger;
            _jwtHandlerFactory = jwtHandlerFactory;
            _dbContext = dbContext;
        }

        public IJwtRevocation CreateStrategy()
        {
            _logger.LogInformation("Creating a JWT revocation strategy");

            return _jwtRevocationStrategyOptions.StrategyName switch
            {
                JwtRevocationStrategyConstants.JtiMatchter => new JtiMatcherRevocationStrategyService(_jwtHandlerFactory, _dbContext),
                JwtRevocationStrategyConstants.AllowList => new AllowedListRevocationStrategyService(_jwtHandlerFactory, _dbContext),
                JwtRevocationStrategyConstants.Denylist => new DenyListRevocationStrategyService(_jwtHandlerFactory, _dbContext),
                _ => throw new InvalidOperationException("No JWT Revocation Strategy was set")
            };

        }
    }
}
