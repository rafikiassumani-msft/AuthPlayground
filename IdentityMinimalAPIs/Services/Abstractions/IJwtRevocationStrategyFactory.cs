namespace IdentityMinimalAPIs.Services.Abstractions
{
    public interface IJwtRevocationStrategyFactory
    {
        IJwtRevocation CreateStrategy();
    }
}
