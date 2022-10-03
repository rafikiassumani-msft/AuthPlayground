namespace IdentityMinimalAPIs.DTOs
{
    public class JwtRevocationRequestDTO
    {
        public string UserId { get; set; }

        public string JwtToken { get; set; }
    }
}
