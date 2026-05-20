namespace me_workspace.Web.Features.Files;

public sealed class FileContextService(IWebHostEnvironment environment)
{
    private static readonly string[] ApprovedRoots =
    [
        "docs",
        "Journals",
        "ThingsToDo",
        "src/me_workspace.Web",
        "tests"
    ];

    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "obj",
        ".git"
    };

    public IReadOnlyList<FileTreeNodeDto> GetApprovedTree()
    {
        var workspaceRoot = GetWorkspaceRoot();
        var nodes = new List<FileTreeNodeDto>();

        foreach (var approvedRoot in ApprovedRoots)
        {
            var absolutePath = Path.Combine(workspaceRoot, approvedRoot.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(absolutePath))
            {
                continue;
            }

            nodes.Add(BuildDirectoryNode(absolutePath, NormalizeRelativePath(approvedRoot)));
        }

        return nodes;
    }

    public async Task<FilePreviewDto?> GetFilePreviewAsync(string relativePath, CancellationToken cancellationToken)
    {
        var filePath = ResolveApprovedFilePath(relativePath);
        if (filePath is null || !File.Exists(filePath))
        {
            return null;
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        if (content.Contains('\0'))
        {
            return new FilePreviewDto(
                Path.GetFileName(filePath),
                NormalizeRelativePath(relativePath),
                "Binary file preview is not supported in the first folder view.",
                false);
        }

        const int maxLength = 4000;
        var truncated = content.Length > maxLength;
        var preview = truncated ? $"{content[..maxLength].TrimEnd()}\n\n[Preview truncated]" : content;

        return new FilePreviewDto(
            Path.GetFileName(filePath),
            NormalizeRelativePath(relativePath),
            preview,
            truncated);
    }

    public async Task<IReadOnlyList<FileContextSnippetDto>> GetContextSnippetsAsync(IEnumerable<string>? relativePaths, CancellationToken cancellationToken)
    {
        if (relativePaths is null)
        {
            return [];
        }

        var snippets = new List<FileContextSnippetDto>();

        foreach (var relativePath in relativePaths
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(NormalizeRelativePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3))
        {
            var preview = await GetFilePreviewAsync(relativePath, cancellationToken);
            if (preview is null)
            {
                continue;
            }

            snippets.Add(new FileContextSnippetDto(preview.RelativePath, preview.ContentPreview));
        }

        return snippets;
    }

    private FileTreeNodeDto BuildDirectoryNode(string directoryPath, string relativePath)
    {
        var children = new List<FileTreeNodeDto>();

        foreach (var childDirectory in Directory.GetDirectories(directoryPath).OrderBy(Path.GetFileName))
        {
            var name = Path.GetFileName(childDirectory);
            if (IgnoredDirectories.Contains(name))
            {
                continue;
            }

            var childRelativePath = NormalizeRelativePath(Path.Combine(relativePath, name));
            children.Add(BuildDirectoryNode(childDirectory, childRelativePath));
        }

        foreach (var filePath in Directory.GetFiles(directoryPath).OrderBy(Path.GetFileName))
        {
            var name = Path.GetFileName(filePath);
            var fileRelativePath = NormalizeRelativePath(Path.Combine(relativePath, name));
            children.Add(new FileTreeNodeDto(name, fileRelativePath, false, []));
        }

        return new FileTreeNodeDto(Path.GetFileName(directoryPath), relativePath, true, children);
    }

    private string GetWorkspaceRoot() =>
        Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", ".."));

    private string? ResolveApprovedFilePath(string relativePath)
    {
        var workspaceRoot = GetWorkspaceRoot();
        var normalizedRelativePath = NormalizeRelativePath(relativePath);
        var fullPath = Path.GetFullPath(Path.Combine(workspaceRoot, normalizedRelativePath.Replace('/', Path.DirectorySeparatorChar)));

        foreach (var approvedRoot in ApprovedRoots)
        {
            var fullApprovedRoot = Path.GetFullPath(Path.Combine(workspaceRoot, approvedRoot.Replace('/', Path.DirectorySeparatorChar)));
            if (string.Equals(fullPath, fullApprovedRoot, StringComparison.OrdinalIgnoreCase) ||
                fullPath.StartsWith($"{fullApprovedRoot}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static string NormalizeRelativePath(string relativePath) =>
        relativePath.Replace('\\', '/').TrimStart('/');
}
