using me_workspace.Web.Data.Entities;
using me_workspace.Web.Features.Files;
using me_workspace.Web.Features.Journal;
using me_workspace.Web.Features.Memory;

namespace me_workspace.Web.Features.Context;

public sealed class ContextService
{
    public ContextSnapshotDto BuildSnapshot(
        IReadOnlyCollection<Message> messages,
        IReadOnlyList<MemoryItemDto> memoryItems,
        IReadOnlyList<JournalEntryDto> journalEntries,
        IReadOnlyList<FileContextSnippetDto>? fileSnippets = null)
    {
        var recentMessages = messages
            .OrderByDescending(x => x.CreatedUtc)
            .Take(4)
            .OrderBy(x => x.CreatedUtc)
            .Select(x => $"{x.Role}: {x.Content}")
            .ToList();

        var sources = new List<string>
        {
            $"recent-messages:{recentMessages.Count}",
            $"memory:{memoryItems.Count}",
            $"journal:{journalEntries.Count}",
            $"files:{fileSnippets?.Count ?? 0}"
        };

        var summaryParts = new List<string>();

        if (recentMessages.Count > 0)
        {
            summaryParts.Add($"Recent conversation: {string.Join(" | ", recentMessages)}");
        }

        if (memoryItems.Count > 0)
        {
            summaryParts.Add($"Pinned memory: {string.Join(" | ", memoryItems.Select(x => x.Content))}");
        }

        if (journalEntries.Count > 0)
        {
            summaryParts.Add($"Journal focus: {journalEntries[0].Title} | {journalEntries[0].Summary}");
        }

        if (fileSnippets is not null && fileSnippets.Count > 0)
        {
            var fileSummary = string.Join(
                " | ",
                fileSnippets.Select(x => $"{x.RelativePath}: {x.ContentSnippet.Replace(Environment.NewLine, " ")}"));
            summaryParts.Add($"Attached files: {fileSummary}");
        }

        var summary = string.Join(" ", summaryParts);

        return new ContextSnapshotDto(sources, summary);
    }

    public string BuildPhaseOneSummary() =>
        "The local flow now combines recent chat, pinned memory, journal focus, and any attached file snippets before the assistant reply is generated.";
}
