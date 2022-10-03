namespace IdentityMinimalAPIs.DTOs
{
    public class LoginResponseDTO
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set;}
        public bool? AccountLocked { get; set; }
        public bool? RequiredTwoFactor { get; set; }
        public bool? TwoFactorAuthSatisfied { get; set; }
        public string? TokenType { get; set; }   
        public IList<string>? EnabledMfas { get; set; }
        public string? Email { get; set; }
    }
}
