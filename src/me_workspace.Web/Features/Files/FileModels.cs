namespace me_workspace.Web.Features.Files;

public sealed record FileTreeNodeDto(
    string Name,
    string RelativePath,
    bool IsDirectory,
    IReadOnlyList<FileTreeNodeDto> Children);

public sealed record FilePreviewDto(
    string Name,
    string RelativePath,
    string ContentPreview,
    bool Truncated);

public sealed record FileContextSnippetDto(
    string RelativePath,
    string ContentSnippet);
