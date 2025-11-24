using Microsoft.EntityFrameworkCore;
using Groups.Service.Models;

namespace Groups.Service.Data;

public class GroupsDbContext : DbContext
{
    public GroupsDbContext(DbContextOptions<GroupsDbContext> options) : base(options)
    {
    }

    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasIndex(e => e.CreatedByUserId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasMany(g => g.Members)
                .WithOne(gm => gm.Group)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasIndex(e => new { e.GroupId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
        });
    }
}
