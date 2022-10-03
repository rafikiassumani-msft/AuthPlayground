namespace IdentityMinimalAPIs.Services.ConfigExtensions
{
    public class JwtRevocationStrategyOptions
    {
        public string StrategyName { get; set; }
    }

    public class JwtRevocationStrategyConstants
    {
        public const string JtiMatchter = "JtiMatchter";
        public const string Denylist = "Denylist";
        public const string AllowList = "AllowList";
    }
}