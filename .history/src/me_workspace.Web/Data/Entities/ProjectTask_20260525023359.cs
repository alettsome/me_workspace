namespace me_workspace.Web.Data.Entities;

/// <summary>
/// Represents a task or action item within a project.
/// Can be linked to document anchors for traceability.
/// </summary>
public sealed class ProjectTask
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The project this task belongs to
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Optional: The document anchor this task is tied to
    /// </summary>
    public Guid? AnchorId { get; set; }

    /// <summary>
    /// Task description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Status: "pending", "in-progress", "complete", "blocked"
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Priority: "low", "medium", "high", "critical"
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Optional due date
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Optional notes or additional context
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Link to conversation or log entry where this task originated
    /// </summary>
    public Guid? ConversationId { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedUtc { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public DocumentAnchor? Anchor { get; set; }
    public Conversation? Conversation { get; set; }
}
