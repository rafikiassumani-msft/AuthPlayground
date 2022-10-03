namespace IdentityMinimalAPIs.DTOs
{
    public class ResetPasswordDTO
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ResetPasswordToken { get; set; }
    }
}
