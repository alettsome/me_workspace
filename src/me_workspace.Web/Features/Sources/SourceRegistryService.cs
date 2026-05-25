using Microsoft.EntityFrameworkCore;
using me_workspace.Web.Data;
using me_workspace.Web.Data.Entities;
using me_workspace.Web.Infrastructure.Workspace;

namespace me_workspace.Web.Features.Sources;

public sealed class SourceRegistryService(
    AppDbContext db,
    WorkspaceLayoutService workspaceLayoutService,
    ILogger<SourceRegistryService> logger)
{
    public async Task<IReadOnlyList<SourceListItemDto>> GetSourcesAsync(CancellationToken cancellationToken)
    {
        return await db.Sources
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedUtc)
            .Select(x => new SourceListItemDto(
                x.Id,
                x.SourceKey,
                x.Title,
                x.SourceType,
                x.RightsLabel,
                x.OriginalRelativePath,
                x.CurrentStage,
                x.Status,
                x.CreatedUtc,
                x.UpdatedUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<SourceDetailDto?> GetSourceAsync(Guid id, CancellationToken cancellationToken)
    {
        var source = await db.Sources
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Files)
            .Include(x => x.Jobs)
            .Include(x => x.Tags)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return source is null
            ? null
            : new SourceDetailDto(
                source.Id,
                source.SourceKey,
                source.Title,
                source.SourceType,
                source.RightsLabel,
                source.OriginalRelativePath,
                source.CurrentStage,
                source.Status,
                source.CreatedUtc,
                source.UpdatedUtc,
                source.Files
                    .OrderBy(x => x.CreatedUtc)
                    .Select(x => new SourceFileDto(x.Id, x.RelativePath, x.FileRole, x.CreatedUtc))
                    .ToList(),
                source.Jobs
                    .OrderByDescending(x => x.StartedUtc)
                    .Select(x => new ProcessingJobDto(x.Id, x.JobType, x.Status, x.StartedUtc, x.CompletedUtc, x.ErrorMessage))
                    .ToList(),
                source.Tags
                    .OrderBy(x => x.Tag)
                    .Select(x => x.Tag)
                    .ToList());
    }

    public async Task<PipelineScanResultDto> ScanInboxAsync(CancellationToken cancellationToken)
    {
        workspaceLayoutService.EnsureRuntimeFolders();

        var inboxFiles = Directory.EnumerateFiles(workspaceLayoutService.InboxRoot, "*", SearchOption.TopDirectoryOnly)
            .OrderBy(Path.GetFileName)
            .ToList();

        var registeredSources = new List<SourceListItemDto>();

        foreach (var absolutePath in inboxFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(workspaceLayoutService.RuntimeRoot, absolutePath)
                .Replace('\\', '/');

            var alreadyRegistered = await db.Sources
                .AsNoTracking()
                .AnyAsync(x => x.OriginalRelativePath == relativePath, cancellationToken);

            if (alreadyRegistered)
            {
                continue;
            }

            var now = DateTime.UtcNow;
            var source = new Source
            {
                SourceKey = CreateSourceKey(now),
                Title = Path.GetFileNameWithoutExtension(absolutePath),
                SourceType = InferSourceType(absolutePath),
                RightsLabel = "unclassified",
                OriginalRelativePath = relativePath,
                CurrentStage = "Inbox",
                Status = "Queued",
                CreatedUtc = now,
                UpdatedUtc = now,
                Files =
                [
                    new SourceFile
                    {
                        RelativePath = relativePath,
                        FileRole = "original",
                        CreatedUtc = now
                    }
                ],
                Jobs =
                [
                    new ProcessingJob
                    {
                        JobType = "intake",
                        Status = "Completed",
                        StartedUtc = now,
                        CompletedUtc = now
                    }
                ]
            };

            db.Sources.Add(source);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Registered inbox source {SourceKey} from {RelativePath}.",
                source.SourceKey,
                relativePath);

            registeredSources.Add(new SourceListItemDto(
                source.Id,
                source.SourceKey,
                source.Title,
                source.SourceType,
                source.RightsLabel,
                source.OriginalRelativePath,
                source.CurrentStage,
                source.Status,
                source.CreatedUtc,
                source.UpdatedUtc));
        }

        return new PipelineScanResultDto(registeredSources.Count, registeredSources);
    }

    public string CreateSourceKey(DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        var timestamp = now.ToString("yyyyMMdd-HHmmss");
        var suffix = Guid.NewGuid().ToString("N")[..6];
        return $"src-{timestamp}-{suffix}";
    }

    private static string InferSourceType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".txt" => "Text",
            ".md" => "Markdown",
            ".pdf" => "Pdf",
            ".json" => "ChatExport",
            _ => "Text"
        };
    }
}
