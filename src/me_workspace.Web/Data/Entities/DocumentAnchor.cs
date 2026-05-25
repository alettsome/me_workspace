namespace me_workspace.Web.Data.Entities;

/// <summary>
/// Represents an anchor point in a master document template.
/// Anchors define the structure and serve as attachment points for tasks and content.
/// </summary>
public sealed class DocumentAnchor
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The project this anchor belongs to
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Human-readable anchor name (e.g., "Chapter 1: Vitamin Basics")
    /// </summary>
    public string AnchorName { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for referencing in markdown (e.g., "chapter-1-vitamin-basics")
    /// </summary>
    public string AnchorKey { get; set; } = string.Empty;

    /// <summary>
    /// Order/position in the document (for sorting)
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Status: "pending", "in-progress", "complete"
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Optional description or notes about this section
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Estimated completion percentage (0-100)
    /// </summary>
    public int? CompletionPercent { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Project Project { get; set; } = null!;
    public List<ProjectTask> Tasks { get; set; } = [];
}
