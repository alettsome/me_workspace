namespace me_workspace.Web.Data.Entities;

public sealed class MemoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Key { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public bool Pinned { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
