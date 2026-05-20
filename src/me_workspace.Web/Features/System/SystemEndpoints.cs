using me_workspace.Web.Features.Memory;
using me_workspace.Web.Features.Voice;

namespace me_workspace.Web.Features.System;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/system/flow", async (SystemService systemService, CancellationToken cancellationToken) =>
        {
            var flow = await systemService.GetFlowAsync(cancellationToken);
            return Results.Ok(flow);
        });

        app.MapGet("/api/memory/items", async (MemoryService memoryService, CancellationToken cancellationToken) =>
        {
            var items = await memoryService.GetAllItemsAsync(cancellationToken);
            return Results.Ok(items);
        });

        app.MapPost("/api/memory/items", async (CreateMemoryItemRequest request, MemoryService memoryService, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Content))
            {
                return Results.BadRequest(new { error = "Memory items need both a key and content." });
            }

            var item = await memoryService.CreateItemAsync(request, cancellationToken);
            return Results.Created($"/api/memory/items/{item!.Id}", item);
        });

        app.MapPut("/api/memory/items/{id:guid}", async (Guid id, UpdateMemoryItemRequest request, MemoryService memoryService, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Content))
            {
                return Results.BadRequest(new { error = "Memory items need both a key and content." });
            }

            var item = await memoryService.UpdateItemAsync(id, request, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        app.MapDelete("/api/memory/items/{id:guid}", async (Guid id, MemoryService memoryService, CancellationToken cancellationToken) =>
        {
            var deleted = await memoryService.DeleteItemAsync(id, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        app.MapPost("/api/voice/demo-transcript", async (VoiceService voiceService, CancellationToken cancellationToken) =>
        {
            var transcript = await voiceService.CreateDraftTranscriptAsync(cancellationToken);
            return Results.Ok(transcript);
        });

        return app;
    }
}
