using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YourAppName.Data.Entities;
using YourAppName.Data.Entities.Identity;

namespace YourAppName.Infrastructure.Data
{
    // Inherit from IdentityDbContext if you are using ASP.NET Core Identity for users
    public class AppDbContext : IdentityDbContext<
        ApplicationUser,                // 1. TUser: Your custom user class
        ApplicationRole,                // 2. TRole: Your custom role class
        string,                         // 3. TKey: The primary key type (string for GUID)
        IdentityUserClaim<string>,      // 4. TUserClaim
        IdentityUserRole<string>,       // 5. TUserRole
        IdentityUserLogin<string>,      // 6. TUserLogin
        IdentityRoleClaim<string>,      // 7. TRoleClaim
        IdentityUserToken<string>       // 8. TUserToken
        >
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<Country> Countries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
        }
    }
}