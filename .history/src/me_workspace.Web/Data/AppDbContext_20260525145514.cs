using Microsoft.EntityFrameworkCore;
using me_workspace.Web.Data.Entities;

namespace me_workspace.Web.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Source> Sources => Set<Source>();

    public DbSet<SourceFile> SourceFiles => Set<SourceFile>();

    public DbSet<SourceTag> SourceTags => Set<SourceTag>();

    public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();

    public DbSet<Chunk> Chunks => Set<Chunk>();

    public DbSet<Summary> Summaries => Set<Summary>();

    public DbSet<DocumentAnchor> DocumentAnchors => Set<DocumentAnchor>();

    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();

    public DbSet<AgentLog> AgentLogs => Set<AgentLog>();

    public DbSet<TrendAnalysis> TrendAnalyses => Set<TrendAnalysis>();

    public DbSet<ProcessingNotification> ProcessingNotifications => Set<ProcessingNotification>();

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

        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceKey).HasMaxLength(64);
            entity.Property(x => x.Title).HasMaxLength(300);
            entity.Property(x => x.SourceType).HasMaxLength(64);
            entity.Property(x => x.Author).HasMaxLength(200);
            entity.Property(x => x.ISBN).HasMaxLength(20);
            entity.Property(x => x.URL).HasMaxLength(1000);
            entity.Property(x => x.Publisher).HasMaxLength(200);
            entity.Property(x => x.BorrowingSource).HasMaxLength(100);
            entity.Property(x => x.RightsLabel).HasMaxLength(64);
            entity.Property(x => x.OriginalRelativePath).HasMaxLength(500);
            entity.Property(x => x.CurrentStage).HasMaxLength(64);
            entity.Property(x => x.Status).HasMaxLength(32);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.Property(x => x.UpdatedUtc).IsRequired();
            entity.HasIndex(x => x.SourceKey).IsUnique();
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.AccessExpiryUtc);
            entity.HasOne(x => x.Project)
                .WithMany(x => x.Sources)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SourceFile>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RelativePath).HasMaxLength(500);
            entity.Property(x => x.FileRole).HasMaxLength(32);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.HasOne(x => x.Source)
                .WithMany(x => x.Files)
                .HasForeignKey(x => x.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SourceTag>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Tag).HasMaxLength(80);
            entity.HasOne(x => x.Source)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProcessingJob>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.JobType).HasMaxLength(32);
            entity.Property(x => x.Status).HasMaxLength(32);
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.Property(x => x.StartedUtc).IsRequired();
            entity.HasOne(x => x.Source)
                .WithMany(x => x.Jobs)
                .HasForeignKey(x => x.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Chunk>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SectionTitle).HasMaxLength(300);
            entity.Property(x => x.PageReference).HasMaxLength(64);
            entity.Property(x => x.TextPath).HasMaxLength(500);
            entity.Property(x => x.Status).HasMaxLength(32);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.HasOne(x => x.Source)
                .WithMany(x => x.Chunks)
                .HasForeignKey(x => x.SourceId)
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

        // New entities for Phase 6

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Timeline).HasMaxLength(100);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.FolderPath).HasMaxLength(500);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.Property(x => x.UpdatedUtc).IsRequired();
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<Summary>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SummaryText).IsRequired();
            entity.Property(x => x.PageRange).HasMaxLength(100);
            entity.Property(x => x.Keywords).HasMaxLength(1000);
            entity.Property(x => x.EmbeddingModel).HasMaxLength(100);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.HasOne(x => x.Source)
                .WithMany(x => x.Summaries)
                .HasForeignKey(x => x.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Project)
                .WithMany(x => x.Summaries)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => x.SourceId);
        });

        modelBuilder.Entity<DocumentAnchor>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AnchorName).HasMaxLength(300).IsRequired();
            entity.Property(x => x.AnchorKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.Property(x => x.UpdatedUtc).IsRequired();
            entity.HasOne(x => x.Project)
                .WithMany(x => x.Anchors)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Position });
            entity.HasIndex(x => new { x.ProjectId, x.AnchorKey }).IsUnique();
        });

        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Priority).HasMaxLength(32);
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.Property(x => x.UpdatedUtc).IsRequired();
            entity.HasOne(x => x.Project)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Anchor)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.AnchorId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Conversation)
                .WithMany()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<AgentLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AgentType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.HasOne(x => x.Project)
                .WithMany(x => x.AgentLogs)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => x.CreatedUtc);
        });

        modelBuilder.Entity<TrendAnalysis>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Topic).HasMaxLength(300).IsRequired();
            entity.Property(x => x.TrendSummary).IsRequired();
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.HasOne(x => x.Project)
                .WithMany(x => x.TrendAnalyses)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => x.Topic);
        });
    }
}
