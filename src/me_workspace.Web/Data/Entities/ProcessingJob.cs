namespace me_workspace.Web.Data.Entities;

public sealed class ProcessingJob
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SourceId { get; set; }

    public string JobType { get; set; } = string.Empty;

    public string Status { get; set; } = "New";

    public DateTime StartedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedUtc { get; set; }

    public string? ErrorMessage { get; set; }

    public Source? Source { get; set; }
}
