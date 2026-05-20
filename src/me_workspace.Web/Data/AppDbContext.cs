using Microsoft.EntityFrameworkCore;
using me_workspace.Web.Data.Entities;

namespace me_workspace.Web.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<MemoryItem> MemoryItems => Set<MemoryItem>();

    public DbSet<MessageFileContext> MessageFileContexts => Set<MessageFileContext>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.JournalEntryId);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.Property(x => x.UpdatedUtc).IsRequired();
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Role).HasMaxLength(32);
            entity.Property(x => x.Content).HasMaxLength(8000);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.HasOne(x => x.Conversation)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MemoryItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(80);
            entity.Property(x => x.Content).HasMaxLength(4000);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.Property(x => x.UpdatedUtc).IsRequired();
        });

        modelBuilder.Entity<MessageFileContext>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RelativePath).HasMaxLength(500);
            entity.Property(x => x.ContentSnippet).HasMaxLength(4000);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.HasOne(x => x.Message)
                .WithMany(x => x.FileContexts)
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
