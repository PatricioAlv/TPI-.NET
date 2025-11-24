using Microsoft.EntityFrameworkCore;
using Messages.Service.Models;

namespace Messages.Service.Data;

public class MessagesDbContext : DbContext
{
    public MessagesDbContext(DbContextOptions<MessagesDbContext> options) : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }
    public DbSet<ChatParticipant> ChatParticipants { get; set; }
    public DbSet<MessageRead> MessageReads { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasIndex(e => e.SenderId);
            entity.HasIndex(e => e.ReceiverId);
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.SentAt);
        });

        modelBuilder.Entity<ChatParticipant>(entity =>
        {
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.Username);
        });

        modelBuilder.Entity<MessageRead>(entity =>
        {
            entity.HasIndex(e => new { e.MessageId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.MessageId);
            entity.HasIndex(e => e.UserId);
            
            entity.HasOne(mr => mr.Message)
                .WithMany()
                .HasForeignKey(mr => mr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
