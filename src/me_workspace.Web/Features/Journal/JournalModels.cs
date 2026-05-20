namespace me_workspace.Web.Features.Journal;

public sealed record JournalEntryDto(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    DateTime UpdatedUtc,
    int LogCount);

public sealed record JournalLogDto(
    string Name,
    string RelativePath,
    DateTime UpdatedUtc);

public sealed record JournalEntryDetailDto(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string EntryContent,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
    IReadOnlyList<string> Tags,
    IReadOnlyList<JournalLogDto> Logs);

public sealed record CreateJournalEntryRequest(string Title, string? Summary, IReadOnlyList<string>? Tags);
