using me_workspace.Web.Features.Context;
using me_workspace.Web.Features.Files;
using me_workspace.Web.Features.Journal;
using me_workspace.Web.Features.Memory;
using me_workspace.Web.Features.Voice;

namespace me_workspace.Web.Features.System;

public sealed class SystemService(
    ContextService contextService,
    FileContextService fileContextService,
    MemoryService memoryService,
    JournalService journalService,
    VoiceService voiceService)
{
    public async Task<SystemFlowDto> GetFlowAsync(CancellationToken cancellationToken)
    {
        var approvedRoots = fileContextService.GetApprovedTree();
        var memoryItems = await memoryService.GetPinnedItemsAsync(cancellationToken);
        var journalEntries = await journalService.GetRecentEntriesAsync(cancellationToken);
        var context = contextService.BuildSnapshot([], memoryItems, journalEntries);

        IReadOnlyList<FeatureConnectionDto> connections =
        [
            new("chat", "connected", "Browser UI posts messages to the local ASP.NET Core backend and stores them in SQLite."),
            new("context", "connected", contextService.BuildPhaseOneSummary()),
            new("files", "connected", $"{approvedRoots.Count} approved root areas are available in the folder view."),
            new("memory", "connected", $"{memoryItems.Count} pinned local memory items are available to the assistant flow."),
            new("journal", "connected", $"{journalEntries.Count} journal entr{(journalEntries.Count == 1 ? "y is" : "ies are")} available as focus context and folder-backed continuity."),
            new("voice", "connected", voiceService.GetStatusSummary())
        ];

        return new SystemFlowDto(
            "me_workspace",
            "me_workspace is the app name for this local workspace assistant flow.",
            context,
            connections);
    }
}
