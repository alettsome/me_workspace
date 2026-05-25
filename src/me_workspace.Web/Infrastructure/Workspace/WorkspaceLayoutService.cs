using Microsoft.Extensions.Options;

namespace me_workspace.Web.Infrastructure.Workspace;

public sealed class WorkspaceLayoutService(IOptions<WorkspaceOptions> options)
{
    private readonly WorkspaceOptions _options = options.Value;

    public string RuntimeRoot => _options.RuntimeRoot;

    public string InboxRoot => GetPath(_options.InboxFolderName);

    public string NormalizedRoot => GetPath(_options.NormalizedFolderName);

    public string ChunkedRoot => GetPath(_options.ChunkedFolderName);

    public string SummariesRoot => GetPath(_options.SummariesFolderName);

    public string ReviewedRoot => GetPath(_options.ReviewedFolderName);

    public string OutputsRoot => GetPath(_options.OutputsFolderName);

    public string ArchiveRoot => GetPath(_options.ArchiveFolderName);

    public string LogsRoot => GetPath(_options.LogsFolderName);

    public string TempRoot => GetPath(_options.TempFolderName);

    public IReadOnlyList<string> GetStageDirectories() =>
    [
        InboxRoot,
        NormalizedRoot,
        ChunkedRoot,
        SummariesRoot,
        ReviewedRoot,
        OutputsRoot,
        ArchiveRoot,
        LogsRoot,
        TempRoot
    ];

    public void EnsureRuntimeFolders()
    {
        Directory.CreateDirectory(RuntimeRoot);

        foreach (var directory in GetStageDirectories())
        {
            Directory.CreateDirectory(directory);
        }
    }

    public string GetPath(string folderName) => Path.Combine(RuntimeRoot, folderName);
}
