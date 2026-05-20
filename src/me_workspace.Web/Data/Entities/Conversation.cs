namespace me_workspace.Web.Data.Entities;

public sealed class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "New Chat";

    public Guid? JournalEntryId { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    public List<Message> Messages { get; set; } = [];
}
