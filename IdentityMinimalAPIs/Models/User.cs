using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityMinimalAPIs.Models
{
    [Index(nameof(Jti))]
    public class User: IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Jti { get; set; }
        public string? Preferred2fa { get; set; }
    }
}
