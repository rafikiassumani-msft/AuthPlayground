namespace IdentityMinimalAPIs.DTOs
{
    public class UserRequestDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Telephone { get; set; }
        public bool Enabled2fa { get; set; }
    }
}
