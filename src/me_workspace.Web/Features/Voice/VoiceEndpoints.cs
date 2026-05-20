namespace me_workspace.Web.Features.Voice;

public static class VoiceEndpoints
{
    public static IEndpointRouteBuilder MapVoiceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/voice/demo-transcript", async (VoiceService voiceService, CancellationToken cancellationToken) =>
        {
            var transcript = await voiceService.CreateDraftTranscriptAsync(cancellationToken);
            return Results.Ok(transcript);
        });

        app.MapPost("/api/voice/sessions/start", async (VoiceService voiceService, CancellationToken cancellationToken) =>
        {
            var session = await voiceService.StartSessionAsync(cancellationToken);
            return Results.Ok(session);
        });

        app.MapGet("/api/voice/sessions/{id:guid}", async (Guid id, VoiceService voiceService, CancellationToken cancellationToken) =>
        {
            var session = await voiceService.GetSessionAsync(id, cancellationToken);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        app.MapPost("/api/voice/sessions/{id:guid}/chunks", async (Guid id, HttpRequest httpRequest, VoiceService voiceService, CancellationToken cancellationToken) =>
        {
            var session = await voiceService.AppendAudioChunkAsync(id, httpRequest.Body, cancellationToken);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        app.MapPost("/api/voice/sessions/{id:guid}/stop", async (Guid id, VoiceService voiceService, CancellationToken cancellationToken) =>
        {
            var session = await voiceService.StopSessionAsync(id, cancellationToken);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        return app;
    }
}
