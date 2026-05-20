namespace me_workspace.Web.Features.Chat;

public sealed record CreateConversationRequest(string? Title, Guid? JournalEntryId);

public sealed record ConversationDto(
    Guid Id,
    string Title,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
    Guid? JournalEntryId,
    string? JournalTitle,
    string? Preview);

public sealed record SendMessageRequest(string Content, IReadOnlyList<string>? FilePaths, Guid? JournalEntryId);

public sealed record MessageFileContextDto(Guid Id, string RelativePath, string ContentSnippet, DateTime CreatedUtc);

public sealed record MessageDto(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedUtc,
    IReadOnlyList<MessageFileContextDto> FileContexts);

public sealed record ConversationDetailDto(
    Guid Id,
    string Title,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
    Guid? JournalEntryId,
    string? JournalTitle,
    IReadOnlyList<MessageDto> Messages);
