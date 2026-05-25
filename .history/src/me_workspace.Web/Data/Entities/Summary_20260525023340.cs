namespace me_workspace.Web.Data.Entities;

/// <summary>
/// Represents a summarized excerpt from a source, optionally enriched with embeddings
/// for semantic search capabilities.
/// </summary>
public sealed class Summary
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The source this summary was generated from
    /// </summary>
    public Guid SourceId { get; set; }

    /// <summary>
    /// The project this summary belongs to (may be shared across projects)
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// The summarized text content
    /// </summary>
    public string SummaryText { get; set; } = string.Empty;

    /// <summary>
    /// Page range or section reference in source (e.g., "p. 45-67", "Chapter 3")
    /// </summary>
    public string? PageRange { get; set; }

    /// <summary>
    /// Metadata keywords extracted from summary (comma-separated or JSON)
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Embedding vector for semantic search (stored as BLOB)
    /// Format: JSON array or binary representation of float[]
    /// Populated in Phase 12 when semantic search is added
    /// </summary>
    public byte[]? EmbeddingVector { get; set; }

    /// <summary>
    /// Model used to generate embedding (e.g., "all-MiniLM-L6-v2")
    /// </summary>
    public string? EmbeddingModel { get; set; }

    /// <summary>
    /// Relevance score or confidence from summarization process
    /// </summary>
    public double? RelevanceScore { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Source Source { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
