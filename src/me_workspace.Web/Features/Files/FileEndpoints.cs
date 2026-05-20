namespace me_workspace.Web.Features.Files;

public static class FileEndpoints
{
    public static IEndpointRouteBuilder MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/files/tree", (FileContextService fileContextService) =>
        {
            var tree = fileContextService.GetApprovedTree();
            return Results.Ok(tree);
        });

        app.MapGet("/api/files/preview", async (string path, FileContextService fileContextService, CancellationToken cancellationToken) =>
        {
            var preview = await fileContextService.GetFilePreviewAsync(path, cancellationToken);
            return preview is null ? Results.NotFound() : Results.Ok(preview);
        });

        return app;
    }
}
