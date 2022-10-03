namespace IdentityMinimalAPIs.Models
{
    public class DenyJwt
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Jti { get; set; }
        public DateTime? ExpirationTime { get; set; }
    }
}
