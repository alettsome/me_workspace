namespace me_workspace.Web.Data.Entities;

public sealed class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }

    public string Role { get; set; } = "user";

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public List<MessageFileContext> FileContexts { get; set; } = [];

    public Conversation? Conversation { get; set; }
}
