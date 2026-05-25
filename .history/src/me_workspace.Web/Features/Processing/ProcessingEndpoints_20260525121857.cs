namespace me_workspace.Web.Features.Processing;

public static class ProcessingEndpoints
{
    public static IEndpointRouteBuilder MapProcessingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/processing/process/{sourceId:guid}", async (
            Guid sourceId, 
            ProcessingPipelineService pipelineService, 
            CancellationToken cancellationToken) =>
        {
            var result = await pipelineService.ProcessSourceAsync(sourceId, cancellationToken);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("ProcessSource")
        .WithOpenApi();

        return app;
    }
}
