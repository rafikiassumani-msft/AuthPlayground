namespace IdentityMinimalAPIs.DTOs
{
    public class TwoFactorAuthRequestDTO
    {
        public string TwoFactorCode { get; set; }
        public string Email { get; set; }
    }
}
