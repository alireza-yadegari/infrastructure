using Auth.Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.DataAccess;

  internal class AuthDbContext : IdentityDbContext<User, Role, Guid>
  {
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    public DbSet<User> MafiaUser { get; set; }

    public DbSet<Role> MafiaRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<User>(entity =>
      {
        entity.ToTable(name: "User");
      });

      builder.Entity<Role>(entity =>
      {
        entity.ToTable(name: "Role");
      });
      builder.Entity<IdentityUserRole<Guid>>(entity =>
      {
        entity.ToTable("UserRoles");
      });

      builder.Entity<IdentityUserClaim<Guid>>(entity =>
      {
        entity.ToTable("UserClaims");
      });

      builder.Entity<IdentityUserLogin<Guid>>(entity =>
      {
        entity.ToTable("UserLogins");
      });

      builder.Entity<IdentityRoleClaim<Guid>>(entity =>
      {
        entity.ToTable("RoleClaims");

      });

      builder.Entity<IdentityUserToken<Guid>>(entity =>
      {
        entity.ToTable("UserTokens");

      });
    }
  }


