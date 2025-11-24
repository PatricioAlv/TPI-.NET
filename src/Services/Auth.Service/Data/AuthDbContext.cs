using Microsoft.EntityFrameworkCore;
using Auth.Service.Models;

namespace Auth.Service.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
        });
    }
}
