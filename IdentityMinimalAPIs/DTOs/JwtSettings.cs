namespace IdentityMinimalAPIs.DTOs
{
    public class JwtSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string TokenSecretKey { get; set; }

    }
}
