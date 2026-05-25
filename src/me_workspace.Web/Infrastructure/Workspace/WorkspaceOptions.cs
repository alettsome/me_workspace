namespace me_workspace.Web.Infrastructure.Workspace;

public sealed class WorkspaceOptions
{
    public const string SectionName = "Workspace";

    public string RuntimeRoot { get; set; } = @"C:\me_workspaces_runtime";

    public string InboxFolderName { get; set; } = "01-Inbox";

    public string NormalizedFolderName { get; set; } = "02-Normalized";

    public string ChunkedFolderName { get; set; } = "03-Chunked";

    public string SummariesFolderName { get; set; } = "04-Summaries";

    public string ReviewedFolderName { get; set; } = "05-Reviewed";

    public string OutputsFolderName { get; set; } = "06-Outputs";

    public string ArchiveFolderName { get; set; } = "07-Archive";

    public string LogsFolderName { get; set; } = "Logs";

    public string TempFolderName { get; set; } = "Temp";

    public int TargetChunkTokens { get; set; } = 700;

    public int ChunkOverlapTokens { get; set; } = 100;
}
