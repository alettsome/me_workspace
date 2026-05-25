using me_workspace.Web.Infrastructure.Workspace;

namespace me_workspace.Web.Features.Files;

public sealed class SourceFileMover(WorkspaceLayoutService workspaceLayoutService)
{
    public string GetStagePath(string folderName) => workspaceLayoutService.GetPath(folderName);
}
