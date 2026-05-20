using System.Text;
using System.Text.Json;
using me_workspace.Web.Data.Entities;
using me_workspace.Web.Features.Files;

namespace me_workspace.Web.Features.Journal;

public sealed class JournalService(IWebHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public void EnsureStorage()
    {
        Directory.CreateDirectory(GetJournalsRoot());
        Directory.CreateDirectory(GetThingsToDoRoot());

        if (!Directory.EnumerateDirectories(GetJournalsRoot()).Any())
        {
            CreateEntryAsync(
                new CreateJournalEntryRequest(
                    "Current focus",
                    "Wire the local pieces together, confirm the flow works, then improve each component in place.",
                    ["priority", "local-flow"]),
                CancellationToken.None).GetAwaiter().GetResult();
        }
        else
        {
            WriteIndexFileAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    public async Task<IReadOnlyList<JournalEntryDto>> GetRecentEntriesAsync(CancellationToken cancellationToken)
    {
        var entries = await LoadEntriesAsync(cancellationToken);
        return entries
            .OrderByDescending(x => x.UpdatedUtc)
            .Take(12)
            .ToList();
    }

    public async Task<IReadOnlyList<JournalEntryDto>> GetEntriesForContextAsync(Guid? preferredEntryId, CancellationToken cancellationToken)
    {
        var entries = await LoadEntriesAsync(cancellationToken);
        if (preferredEntryId is null)
        {
            return entries
                .OrderByDescending(x => x.UpdatedUtc)
                .Take(3)
                .ToList();
        }

        var preferred = entries.SingleOrDefault(x => x.Id == preferredEntryId.Value);
        if (preferred is null)
        {
            return entries
                .OrderByDescending(x => x.UpdatedUtc)
                .Take(3)
                .ToList();
        }

        return
        [
            preferred
        ];
    }

    public async Task<JournalEntryDetailDto?> GetEntryAsync(Guid id, CancellationToken cancellationToken)
    {
        var entryFolder = await FindEntryFolderAsync(id, cancellationToken);
        return entryFolder is null
            ? null
            : await LoadEntryDetailAsync(entryFolder, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, JournalEntryDto>> GetEntryLookupAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken)
    {
        var requestedIds = ids
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToHashSet();

        if (requestedIds.Count == 0)
        {
            return new Dictionary<Guid, JournalEntryDto>();
        }

        var entries = await LoadEntriesAsync(cancellationToken);
        return entries
            .Where(x => requestedIds.Contains(x.Id))
            .ToDictionary(x => x.Id);
    }

    public async Task<JournalEntryDto> CreateEntryAsync(CreateJournalEntryRequest request, CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();
        var summary = string.IsNullOrWhiteSpace(request.Summary)
            ? "New journal entry ready for notes, linked chats, and follow-up work."
            : request.Summary.Trim();

        var existingEntries = await LoadEntriesAsync(cancellationToken);
        var slug = BuildUniqueSlug(title, existingEntries.Select(x => x.Slug));
        var journalId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var entryFolder = Path.Combine(GetJournalsRoot(), slug);
        Directory.CreateDirectory(entryFolder);
        Directory.CreateDirectory(Path.Combine(entryFolder, "logs"));
        Directory.CreateDirectory(Path.Combine(entryFolder, "assets"));
        Directory.CreateDirectory(Path.Combine(entryFolder, "resources"));

        var record = new JournalEntryRecord(
            journalId,
            slug,
            title,
            request.Tags?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [],
            now,
            now);

        await File.WriteAllTextAsync(Path.Combine(entryFolder, "meta.json"), JsonSerializer.Serialize(record, JsonOptions), cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(entryFolder, "summary.md"), $"{summary}{Environment.NewLine}", cancellationToken);
        await File.WriteAllTextAsync(
            Path.Combine(entryFolder, "entry.md"),
            BuildEntryMarkdown(title, summary, record.Tags, now),
            cancellationToken);

        await WriteIndexFileAsync(cancellationToken);

        return new JournalEntryDto(record.Id, record.Slug, record.Title, summary, record.UpdatedUtc, 0);
    }

    public async Task AppendConversationLogAsync(
        Guid journalEntryId,
        Conversation conversation,
        Message userMessage,
        Message assistantMessage,
        IReadOnlyList<FileContextSnippetDto> fileSnippets,
        CancellationToken cancellationToken)
    {
        var entryFolder = await FindEntryFolderAsync(journalEntryId, cancellationToken);
        if (entryFolder is null)
        {
            return;
        }

        var detail = await LoadEntryDetailAsync(entryFolder, cancellationToken);
        var logFolder = Path.Combine(entryFolder, "logs");
        Directory.CreateDirectory(logFolder);

        var logPath = Path.Combine(logFolder, $"chat-{conversation.Id:N}.md");
        var content = BuildConversationLogEntry(conversation, userMessage, assistantMessage, fileSnippets);

        if (!File.Exists(logPath))
        {
            var header = $"# Chat Log{Environment.NewLine}{Environment.NewLine}Journal: {detail.Title}{Environment.NewLine}Conversation: {conversation.Title}{Environment.NewLine}{Environment.NewLine}";
            await File.WriteAllTextAsync(logPath, header, cancellationToken);
        }

        await File.AppendAllTextAsync(logPath, content, cancellationToken);

        var updatedRecord = new JournalEntryRecord(
            detail.Id,
            detail.Slug,
            detail.Title,
            detail.Tags.ToList(),
            detail.CreatedUtc,
            DateTime.UtcNow);

        await File.WriteAllTextAsync(Path.Combine(entryFolder, "meta.json"), JsonSerializer.Serialize(updatedRecord, JsonOptions), cancellationToken);
        await WriteIndexFileAsync(cancellationToken);
    }

    public string GetJournalsRelativeRoot() => "Journals";

    public string GetThingsToDoRelativeRoot() => "ThingsToDo";

    private async Task<IReadOnlyList<JournalEntryDto>> LoadEntriesAsync(CancellationToken cancellationToken)
    {
        var results = new List<JournalEntryDto>();

        foreach (var entryFolder in Directory.EnumerateDirectories(GetJournalsRoot()).OrderBy(Path.GetFileName))
        {
            var detail = await LoadEntryDetailAsync(entryFolder, cancellationToken);
            results.Add(new JournalEntryDto(
                detail.Id,
                detail.Slug,
                detail.Title,
                detail.Summary,
                detail.UpdatedUtc,
                detail.Logs.Count));
        }

        return results;
    }

    private async Task<JournalEntryDetailDto> LoadEntryDetailAsync(string entryFolder, CancellationToken cancellationToken)
    {
        var metaPath = Path.Combine(entryFolder, "meta.json");
        var summaryPath = Path.Combine(entryFolder, "summary.md");
        var entryPath = Path.Combine(entryFolder, "entry.md");
        var logsFolder = Path.Combine(entryFolder, "logs");

        var meta = JsonSerializer.Deserialize<JournalEntryRecord>(
            await File.ReadAllTextAsync(metaPath, cancellationToken),
            JsonOptions)
            ?? throw new InvalidOperationException($"Invalid journal metadata at {metaPath}.");

        var summary = File.Exists(summaryPath)
            ? (await File.ReadAllTextAsync(summaryPath, cancellationToken)).Trim()
            : string.Empty;

        var entryContent = File.Exists(entryPath)
            ? await File.ReadAllTextAsync(entryPath, cancellationToken)
            : string.Empty;

        var logs = Directory.Exists(logsFolder)
            ? Directory.EnumerateFiles(logsFolder, "*.md")
                .Select(path => new FileInfo(path))
                .OrderByDescending(x => x.LastWriteTimeUtc)
                .Take(8)
                .Select(x => new JournalLogDto(
                    x.Name,
                    NormalizeRelativePath(Path.Combine(GetJournalsRelativeRoot(), meta.Slug, "logs", x.Name)),
                    x.LastWriteTimeUtc))
                .ToList()
            : [];

        return new JournalEntryDetailDto(
            meta.Id,
            meta.Slug,
            meta.Title,
            summary,
            entryContent,
            meta.CreatedUtc,
            meta.UpdatedUtc,
            meta.Tags,
            logs);
    }

    private async Task<string?> FindEntryFolderAsync(Guid id, CancellationToken cancellationToken)
    {
        foreach (var entryFolder in Directory.EnumerateDirectories(GetJournalsRoot()))
        {
            var metaPath = Path.Combine(entryFolder, "meta.json");
            if (!File.Exists(metaPath))
            {
                continue;
            }

            var record = JsonSerializer.Deserialize<JournalEntryRecord>(
                await File.ReadAllTextAsync(metaPath, cancellationToken),
                JsonOptions);

            if (record?.Id == id)
            {
                return entryFolder;
            }
        }

        return null;
    }

    private async Task WriteIndexFileAsync(CancellationToken cancellationToken)
    {
        var entries = await LoadEntriesAsync(cancellationToken);
        var index = entries
            .OrderByDescending(x => x.UpdatedUtc)
            .Select(x => new JournalIndexRecord(x.Id, x.Slug, x.Title, x.Summary, x.UpdatedUtc, x.LogCount))
            .ToList();

        var indexPath = Path.Combine(GetJournalsRoot(), "index.json");
        await File.WriteAllTextAsync(indexPath, JsonSerializer.Serialize(index, JsonOptions), cancellationToken);
    }

    private string GetWorkspaceRoot() =>
        Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", ".."));

    private string GetJournalsRoot() =>
        Path.Combine(GetWorkspaceRoot(), GetJournalsRelativeRoot());

    private string GetThingsToDoRoot() =>
        Path.Combine(GetWorkspaceRoot(), GetThingsToDoRelativeRoot());

    private static string BuildUniqueSlug(string title, IEnumerable<string> existingSlugs)
    {
        var existing = existingSlugs.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var baseSlug = BuildSlug(title);
        var slug = baseSlug;
        var counter = 2;

        while (existing.Contains(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private static string BuildSlug(string title)
    {
        var cleaned = new string(title
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray());

        var collapsed = string.Join("-", cleaned.Split('-', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(collapsed) ? "journal-entry" : collapsed;
    }

    private static string BuildEntryMarkdown(string title, string summary, IReadOnlyList<string> tags, DateTime createdUtc)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {title}");
        builder.AppendLine();
        builder.AppendLine($"Created: {createdUtc:O}");
        builder.AppendLine($"Tags: {(tags.Count == 0 ? "none yet" : string.Join(", ", tags))}");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine(summary);
        builder.AppendLine();
        builder.AppendLine("## Notes");
        builder.AppendLine("- Add the next ideas here.");
        builder.AppendLine();
        builder.AppendLine("## Anchors");
        builder.AppendLine("<!-- anchor:overview -->");
        builder.AppendLine();
        builder.AppendLine("## Next");
        builder.AppendLine("- Add follow-up actions in `ThingsToDo/` when this journal produces work.");
        return builder.ToString();
    }

    private static string BuildConversationLogEntry(
        Conversation conversation,
        Message userMessage,
        Message assistantMessage,
        IReadOnlyList<FileContextSnippetDto> fileSnippets)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"## {userMessage.CreatedUtc:O}");
        builder.AppendLine();
        builder.AppendLine($"Conversation ID: `{conversation.Id}`");
        builder.AppendLine();
        builder.AppendLine("### User");
        builder.AppendLine(userMessage.Content);
        builder.AppendLine();

        if (fileSnippets.Count > 0)
        {
            builder.AppendLine("### File Context");
            foreach (var snippet in fileSnippets)
            {
                builder.AppendLine($"- {snippet.RelativePath}");
            }

            builder.AppendLine();
        }

        builder.AppendLine("### Assistant");
        builder.AppendLine(assistantMessage.Content);
        builder.AppendLine();
        return builder.ToString();
    }

    private static string NormalizeRelativePath(string relativePath) =>
        relativePath.Replace('\\', '/').TrimStart('/');

    private sealed record JournalEntryRecord(
        Guid Id,
        string Slug,
        string Title,
        List<string> Tags,
        DateTime CreatedUtc,
        DateTime UpdatedUtc);

    private sealed record JournalIndexRecord(
        Guid Id,
        string Slug,
        string Title,
        string Summary,
        DateTime UpdatedUtc,
        int LogCount);
}
