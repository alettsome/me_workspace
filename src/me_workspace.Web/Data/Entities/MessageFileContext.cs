namespace me_workspace.Web.Data.Entities;

public sealed class MessageFileContext
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MessageId { get; set; }

    public string RelativePath { get; set; } = string.Empty;

    public string ContentSnippet { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public Message? Message { get; set; }
}
