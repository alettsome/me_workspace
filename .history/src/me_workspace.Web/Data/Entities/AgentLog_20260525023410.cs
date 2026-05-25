namespace me_workspace.Web.Data.Entities;

/// <summary>
/// Logs actions performed by AI agents during project phases
/// (assessment, acquisition, analysis, etc.)
/// </summary>
public sealed class AgentLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The project this log entry is associated with
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Type of agent: "researcher", "analyst", "council", "synthesizer", "gap-finder"
    /// </summary>
    public string AgentType { get; set; } = string.Empty;

    /// <summary>
    /// Action performed: "search", "summarize", "analyze", "assess", "recommend"
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Input provided to the agent (query, parameters, etc.)
    /// </summary>
    public string? Input { get; set; }

    /// <summary>
    /// Result or output from the agent action
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// Status: "started", "completed", "failed"
    /// </summary>
    public string Status { get; set; } = "started";

    /// <summary>
    /// Error message if action failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration of action in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Optional metadata (JSON format for extensibility)
    /// </summary>
    public string? Metadata { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedUtc { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
