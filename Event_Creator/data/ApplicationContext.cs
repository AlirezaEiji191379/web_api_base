using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Event_Creator.models.Security;
using Microsoft.EntityFrameworkCore;

namespace Event_Creator.models
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Verification> verifications { get; set; }

        public DbSet<RefreshToken> refreshTokens { get; set; }
        public DbSet<LockedAccount> lockedAccounts { get; set; }
        public DbSet<FailedLogin> failedLogins { get; set; }
        public DbSet<JwtBlackList> jwtBlackLists { get; set; }
        public DbSet<PasswordChange> changePassword { get; set; }


    }
}
