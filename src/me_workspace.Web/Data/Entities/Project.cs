namespace me_workspace.Web.Data.Entities;

/// <summary>
/// Represents a project (book, business plan, strategic plan, etc.)
/// that organizes sources, summaries, and generated documents.
/// </summary>
public sealed class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User-friendly name (e.g., "Health_Fundamentals")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of project: "Book", "BusinessPlan", "StrategicPlan", "Journal"
    /// </summary>
    public string ContentType { get; set; } = "Book";

    /// <summary>
    /// Current status: "idea", "assessment", "acquisition", "active", "complete"
    /// </summary>
    public string Status { get; set; } = "idea";

    /// <summary>
    /// User's target timeline (e.g., "6 months", "Q2 2026")
    /// </summary>
    public string? Timeline { get; set; }

    /// <summary>
    /// Brief description of project goals
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Path to project folder relative to workspace root
    /// </summary>
    public string? FolderPath { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<Source> Sources { get; set; } = [];
    public List<Summary> Summaries { get; set; } = [];
    public List<DocumentAnchor> Anchors { get; set; } = [];
    public List<ProjectTask> Tasks { get; set; } = [];
    public List<AgentLog> AgentLogs { get; set; } = [];
    public List<TrendAnalysis> TrendAnalyses { get; set; } = [];
}
