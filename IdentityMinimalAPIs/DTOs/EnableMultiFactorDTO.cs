namespace IdentityMinimalAPIs.DTOs
{
    public class EnableMultiFactorDTO
    {
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string Selected2fa { get; set; }
    }
}
