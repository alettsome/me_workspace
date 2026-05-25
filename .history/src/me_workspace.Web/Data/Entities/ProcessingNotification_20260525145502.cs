namespace me_workspace.Web.Data.Entities;

/// <summary>
/// Tracks automatic processing notifications
/// </summary>
public class ProcessingNotification
{
    public Guid Id { get; set; }
    public Guid? SourceId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Success", "Failed", "Processing"
    public string? Message { get; set; }
    public int? ChunksCreated { get; set; }
    public long? CharactersProcessed { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public DateTime CreatedUtc { get; set; }
    public bool IsRead { get; set; }

    // Navigation
    public Source? Source { get; set; }
}
