using Microsoft.Extensions.Hosting;

namespace me_workspace.Web.Infrastructure.Workspace;

public sealed class WorkspaceBootstrapHostedService(
    WorkspaceLayoutService workspaceLayoutService,
    ILogger<WorkspaceBootstrapHostedService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        workspaceLayoutService.EnsureRuntimeFolders();

        logger.LogInformation(
            "Workspace runtime folders are ready at {RuntimeRoot}.",
            workspaceLayoutService.RuntimeRoot);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
