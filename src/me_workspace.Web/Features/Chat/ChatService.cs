using Microsoft.EntityFrameworkCore;
using me_workspace.Web.Data;
using me_workspace.Web.Data.Entities;
using me_workspace.Web.Features.Context;
using me_workspace.Web.Features.Files;
using me_workspace.Web.Features.Journal;
using me_workspace.Web.Features.Memory;
using me_workspace.Web.Infrastructure.Llm;

namespace me_workspace.Web.Features.Chat;

public sealed class ChatService(
    AppDbContext db,
    ContextService contextService,
    FileContextService fileContextService,
    MemoryService memoryService,
    JournalService journalService,
    ILlmClient llmClient)
{
    public async Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(CancellationToken cancellationToken)
    {
        var conversations = await db.Conversations
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedUtc)
            .ToListAsync(cancellationToken);

        var conversationIds = conversations.Select(x => x.Id).ToList();
        var previewLookup = await db.Messages
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.ConversationId))
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);

        var latestPreviewByConversation = previewLookup
            .GroupBy(x => x.ConversationId)
            .ToDictionary(
                group => group.Key,
                group => group.First().Content);

        var journalLookup = await journalService.GetEntryLookupAsync(
            conversations.Select(x => x.JournalEntryId),
            cancellationToken);

        return conversations
            .Select(x => new ConversationDto(
                x.Id,
                x.Title,
                x.CreatedUtc,
                x.UpdatedUtc,
                x.JournalEntryId,
                x.JournalEntryId is not null && journalLookup.TryGetValue(x.JournalEntryId.Value, out var entry)
                    ? entry.Title
                    : null,
                latestPreviewByConversation.TryGetValue(x.Id, out var preview)
                    ? preview
                    : null))
            .ToList();
    }

    public async Task<ConversationDto> CreateConversationAsync(CreateConversationRequest? request, CancellationToken cancellationToken)
    {
        var title = string.IsNullOrWhiteSpace(request?.Title) ? "New Chat" : request!.Title.Trim();

        var conversation = new Conversation
        {
            Title = title,
            JournalEntryId = request?.JournalEntryId
        };

        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(cancellationToken);

        var journal = request?.JournalEntryId is null
            ? null
            : await journalService.GetEntryAsync(request.JournalEntryId.Value, cancellationToken);

        return new ConversationDto(
            conversation.Id,
            conversation.Title,
            conversation.CreatedUtc,
            conversation.UpdatedUtc,
            conversation.JournalEntryId,
            journal?.Title,
            null);
    }

    public async Task<ConversationDetailDto?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        var conversation = await db.Conversations
            .AsNoTracking()
            .Include(x => x.Messages)
            .ThenInclude(x => x.FileContexts)
            .SingleOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

        if (conversation is null)
        {
            return null;
        }

        var journal = conversation.JournalEntryId is null
            ? null
            : await journalService.GetEntryAsync(conversation.JournalEntryId.Value, cancellationToken);

        return new ConversationDetailDto(
            conversation.Id,
            conversation.Title,
            conversation.CreatedUtc,
            conversation.UpdatedUtc,
            conversation.JournalEntryId,
            journal?.Title,
            conversation.Messages
                .OrderBy(x => x.CreatedUtc)
                .Select(x => new MessageDto(
                    x.Id,
                    x.Role,
                    x.Content,
                    x.CreatedUtc,
                    x.FileContexts
                        .OrderBy(f => f.CreatedUtc)
                        .Select(f => new MessageFileContextDto(f.Id, f.RelativePath, f.ContentSnippet, f.CreatedUtc))
                        .ToList()))
                .ToList());
    }

    public async Task<ConversationDetailDto?> SendMessageAsync(Guid conversationId, SendMessageRequest request, CancellationToken cancellationToken)
    {
        var trimmedContent = request.Content?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedContent))
        {
            return null;
        }

        var conversation = await db.Conversations
            .Include(x => x.Messages)
            .SingleOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

        if (conversation is null)
        {
            return null;
        }

        Guid? effectiveJournalEntryId = conversation.JournalEntryId;
        if (effectiveJournalEntryId is null && request.JournalEntryId is not null)
        {
            var selectedJournal = await journalService.GetEntryAsync(request.JournalEntryId.Value, cancellationToken);
            if (selectedJournal is not null)
            {
                effectiveJournalEntryId = selectedJournal.Id;
                conversation.JournalEntryId = selectedJournal.Id;
            }
        }

        var fileSnippets = await fileContextService.GetContextSnippetsAsync(request.FilePaths, cancellationToken);

        var userMessage = new Message
        {
            ConversationId = conversation.Id,
            Conversation = conversation,
            Role = "user",
            Content = trimmedContent,
            FileContexts = fileSnippets
                .Select(x => new MessageFileContext
                {
                    RelativePath = x.RelativePath,
                    ContentSnippet = x.ContentSnippet
                })
                .ToList()
        };

        if (conversation.Messages.Count == 1 && conversation.Title == "New Chat")
        {
            conversation.Title = BuildConversationTitle(trimmedContent);
        }

        db.Messages.Add(userMessage);
        var memoryItems = await memoryService.GetPinnedItemsAsync(cancellationToken);
        var journalEntries = await journalService.GetEntriesForContextAsync(effectiveJournalEntryId, cancellationToken);
        var contextSnapshot = contextService.BuildSnapshot(conversation.Messages, memoryItems, journalEntries, fileSnippets);
        var assistantReply = await llmClient.GenerateReplyAsync(
            BuildAssistantPrompt(trimmedContent, contextSnapshot),
            cancellationToken);

        var assistantMessage = new Message
        {
            ConversationId = conversation.Id,
            Conversation = conversation,
            Role = "assistant",
            Content = assistantReply
        };

        db.Messages.Add(assistantMessage);

        conversation.UpdatedUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        if (effectiveJournalEntryId is not null)
        {
            await journalService.AppendConversationLogAsync(
                effectiveJournalEntryId.Value,
                conversation,
                userMessage,
                assistantMessage,
                fileSnippets,
                cancellationToken);
        }

        return await GetConversationAsync(conversationId, cancellationToken);
    }

    private static string BuildAssistantPrompt(string content, ContextSnapshotDto contextSnapshot)
    {
        return string.Join(
            Environment.NewLine,
            $"USER_MESSAGE: {content}",
            $"CONTEXT_SOURCES: {string.Join(", ", contextSnapshot.Sources)}",
            $"CONTEXT_SUMMARY: {contextSnapshot.Summary}");
    }

    private static string BuildConversationTitle(string content)
    {
        const int maxLength = 40;
        var normalized = content.Trim();
        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return $"{normalized[..maxLength].TrimEnd()}...";
    }
}
