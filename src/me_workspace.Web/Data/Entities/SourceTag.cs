namespace me_workspace.Web.Data.Entities;

public sealed class SourceTag
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SourceId { get; set; }

    public string Tag { get; set; } = string.Empty;

    public Source? Source { get; set; }
}
