namespace IdentityMinimalAPIs.DTOs
{
    public class ConfirmPhoneDTO
    {
        public string UserId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? VerificationCode { get; set; }
    }
}
