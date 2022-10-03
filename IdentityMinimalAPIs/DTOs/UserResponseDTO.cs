namespace IdentityMinimalAPIs.DTOs
{
    public class UserResponseDTO
    {
        public string UserId { get; set; }  
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmailConfirmationCode { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? TwoFactorEnabled { get; set; }
        public string? Preferred2fa { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public IList<string>? Available2fas { get; set; }
 
    }
}
