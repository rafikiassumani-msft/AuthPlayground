namespace IdentityMinimalAPIs.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }   
        public DateTime Expiry {get; set;}
        public bool IsRevoked { get; set; }
    }
}
