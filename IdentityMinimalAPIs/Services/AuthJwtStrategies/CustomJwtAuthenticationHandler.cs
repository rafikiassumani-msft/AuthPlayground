using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SharedIdentityServices.Abstractions;
using System.Text.Encodings.Web;
using System.Text.Json;
using IdentityMinimalAPIs.DTOs;
using IdentityMinimalAPIs.Services.Abstractions;

namespace IdentityMinimalAPIs.Services.Auth
{
    public class CustomJwtAuthenticationHandler : AuthenticationHandler<CustomJwtAuthenticationOptions>
    {
        private readonly ITokenService _tokenService;
        private readonly IJwtRevocationStrategyFactory _revocationFactory;

        public CustomJwtAuthenticationHandler(
            IOptionsMonitor<CustomJwtAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ITokenService tokenService,
            IJwtRevocationStrategyFactory revocationFactory) : base(options, logger, encoder, clock)
        {
            _tokenService = tokenService;
            _revocationFactory = revocationFactory;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("No Authorization Header supplied"));
            }

            var authHeader = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ");
            if (authHeader == null || !authHeader.First().Equals("Bearer"))
            {
                return Task.FromResult(AuthenticateResult.Fail("No Bearer token supplied"));
            }

            var accessToken = authHeader.Last();
            var claimsPrincipal = _tokenService.ValidateAccessToken(accessToken);

            if (claimsPrincipal == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid access token supplied"));
            }

            var tokenRevocationHandler = _revocationFactory.CreateStrategy();
            var userId = claimsPrincipal.Claims.First(claim => claim.Type == "userId").Value;
            var jti = claimsPrincipal.Claims.First(claim => claim.Type == "jti").Value;

            var isRevoked = tokenRevocationHandler.IsTokenRevoked(userId, jti);

            if (isRevoked.Result)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid access token"));
            }

            var authTicket = new AuthenticationTicket(claimsPrincipal, "CustomJwt");

            return Task.FromResult(AuthenticateResult.Success(authTicket));
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers.Add("WWW-Authenticate", "Bearer error=Invalid token");
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.ContentType = "application/json";

            var serializedError = JsonSerializer.SerializeToUtf8Bytes(new AuthResultDTO
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Succeeded = false,
                Message = "Invalid bearer token",
                TimeStamp = DateTime.UtcNow,
            });

            await Response.Body.WriteAsync(serializedError);

            await base.HandleChallengeAsync(properties);
        }
    }

    public class CustomJwtAuthenticationOptions : AuthenticationSchemeOptions
    {

    }

}
