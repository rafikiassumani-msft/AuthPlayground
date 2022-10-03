using IdentityMinimalAPIs.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityMinimalAPIs.Data
{
    public class IdentityMinApiDbContext : IdentityDbContext<User>
    {
        public IdentityMinApiDbContext(DbContextOptions<IdentityMinApiDbContext> options) : base(options) { }

        public DbSet<DenyJwt> DenyList => Set<DenyJwt>();
        public DbSet<AllowedJwt> AllowedList => Set<AllowedJwt>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    }
}
