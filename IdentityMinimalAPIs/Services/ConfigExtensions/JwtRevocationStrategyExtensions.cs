using IdentityMinimalAPIs.Services.Abstractions;
using IdentityMinimalAPIs.Services.Auth;

namespace IdentityMinimalAPIs.Services.ConfigExtensions
{
    public static class JwtRevocationStrategyExtensions
    {

        public static IServiceCollection AddJwtRevocationStrategy(this IServiceCollection services, Action<JwtRevocationStrategyOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.Configure(options);
            return services.AddTransient<IJwtRevocationStrategyFactory, JwtRevocationStrategyFactory>();
        }

    }
}
