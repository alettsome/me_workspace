namespace me_workspace.Web.Data.Entities;

public sealed class Chunk
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SourceId { get; set; }

    public int ChunkIndex { get; set; }

    public string? SectionTitle { get; set; }

    public string? PageReference { get; set; }

    public string TextPath { get; set; } = string.Empty;

    public int CharacterCount { get; set; }

    public int TokenCount { get; set; }

    public string Status { get; set; } = "New";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public Source? Source { get; set; }
}
