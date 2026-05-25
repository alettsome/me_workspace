namespace me_workspace.Web.Features.Sources;

public static class SourceEndpoints
{
    public static IEndpointRouteBuilder MapSourceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/sources", async (SourceRegistryService sourceRegistryService, CancellationToken cancellationToken) =>
        {
            var sources = await sourceRegistryService.GetSourcesAsync(cancellationToken);
            return Results.Ok(sources);
        });

        app.MapGet("/api/sources/{id:guid}", async (Guid id, SourceRegistryService sourceRegistryService, CancellationToken cancellationToken) =>
        {
            var source = await sourceRegistryService.GetSourceAsync(id, cancellationToken);
            return source is null ? Results.NotFound() : Results.Ok(source);
        });

        app.MapPost("/api/pipeline/scan", async (SourceRegistryService sourceRegistryService, CancellationToken cancellationToken) =>
        {
            var result = await sourceRegistryService.ScanInboxAsync(cancellationToken);
            return Results.Ok(result);
        });

        return app;
    }
}
