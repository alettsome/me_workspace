namespace me_workspace.Web.Data.Entities;

public sealed class SourceFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SourceId { get; set; }

    public string RelativePath { get; set; } = string.Empty;

    public string FileRole { get; set; } = "original";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public Source? Source { get; set; }
}
