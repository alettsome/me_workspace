namespace me_workspace.Web.Features.Sources;

public sealed record SourceListItemDto(
    Guid Id,
    string SourceKey,
    string Title,
    string SourceType,
    string RightsLabel,
    string OriginalRelativePath,
    string CurrentStage,
    string Status,
    DateTime CreatedUtc,
    DateTime UpdatedUtc);

public sealed record SourceDetailDto(
    Guid Id,
    string SourceKey,
    string Title,
    string SourceType,
    string RightsLabel,
    string OriginalRelativePath,
    string CurrentStage,
    string Status,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
    IReadOnlyList<SourceFileDto> Files,
    IReadOnlyList<ProcessingJobDto> Jobs,
    IReadOnlyList<string> Tags);

public sealed record SourceFileDto(
    Guid Id,
    string RelativePath,
    string FileRole,
    DateTime CreatedUtc);

public sealed record ProcessingJobDto(
    Guid Id,
    string JobType,
    string Status,
    DateTime StartedUtc,
    DateTime? CompletedUtc,
    string? ErrorMessage);

public sealed record PipelineScanResultDto(
    int RegisteredCount,
    IReadOnlyList<SourceListItemDto> RegisteredSources);
