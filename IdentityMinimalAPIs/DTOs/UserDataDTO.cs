
using System.Security.Claims;

namespace IdentityMinimalAPIs.DTOs
{
    internal class UserDataDTO
    {
        public string? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }     
        public bool TwoFactorEnabled { get; set; } 
        public string? Preferred2fa { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public IEnumerable<Claim> Claims { get; set; } = new List<Claim>();
    }
}