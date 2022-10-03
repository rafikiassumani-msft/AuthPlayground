namespace IdentityMinimalAPIs.DTOs
{
    public class Preferred2faDTO
    {
        public string? UserId { get; set; }
        public IList<string> Preferred2fa { get; set; }
    }
}
