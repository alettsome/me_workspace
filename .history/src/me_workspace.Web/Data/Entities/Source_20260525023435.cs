namespace me_workspace.Web.Data.Entities;

public sealed class Source
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Optional: Link to parent project (null for standalone sources)
    /// </summary>
    public Guid? ProjectId { get; set; }

    public string SourceKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Source type: "book", "medical-journal", "web", "article", "text", "pdf"
    /// </summary>
    public string SourceType { get; set; } = "Text";

    /// <summary>
    /// Author name(s)
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// ISBN for books
    /// </summary>
    public string? ISBN { get; set; }

    /// <summary>
    /// URL for web sources or journal articles
    /// </summary>
    public string? URL { get; set; }

    /// <summary>
    /// Publisher name
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Publication year
    /// </summary>
    public int? PublicationYear { get; set; }

    /// <summary>
    /// For library research: source of borrowed material (e.g., "Internet Archive", "Libby")
    /// </summary>
    public string? BorrowingSource { get; set; }

    /// <summary>
    /// For library research: when access expires (borrowed books)
    /// </summary>
    public DateTime? AccessExpiryUtc { get; set; }

    public string RightsLabel { get; set; } = "user-created";

    public string OriginalRelativePath { get; set; } = string.Empty;

    public string CurrentStage { get; set; } = "Inbox";

    /// <summary>
    /// Status: "new", "processing", "summarized", "analyzed", "complete", "error"
    /// </summary>
    public string Status { get; set; } = "New";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Project? Project { get; set; }
    public List<SourceFile> Files { get; set; } = [];
    public List<SourceTag> Tags { get; set; } = [];
    public List<ProcessingJob> Jobs { get; set; } = [];
    public List<Chunk> Chunks { get; set; } = [];
    public List<Summary> Summaries { get; set; } = [];
}
