namespace me_workspace.Web.Features.Memory;

public sealed record MemoryItemDto(
    Guid Id,
    string Key,
    string Content,
    bool Pinned,
    DateTime CreatedUtc,
    DateTime UpdatedUtc);

public sealed record CreateMemoryItemRequest(string Key, string Content, bool Pinned);

public sealed record UpdateMemoryItemRequest(string Key, string Content, bool Pinned);
