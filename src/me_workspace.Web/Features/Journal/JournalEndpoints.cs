namespace me_workspace.Web.Features.Journal;

public static class JournalEndpoints
{
    public static IEndpointRouteBuilder MapJournalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/journal");

        group.MapGet("/entries", async (JournalService journalService, CancellationToken cancellationToken) =>
        {
            var entries = await journalService.GetRecentEntriesAsync(cancellationToken);
            return Results.Ok(entries);
        });

        group.MapGet("/entries/{id:guid}", async (Guid id, JournalService journalService, CancellationToken cancellationToken) =>
        {
            var entry = await journalService.GetEntryAsync(id, cancellationToken);
            return entry is null ? Results.NotFound() : Results.Ok(entry);
        });

        group.MapPost("/entries", async (CreateJournalEntryRequest request, JournalService journalService, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { error = "Journal entries need a title." });
            }

            var entry = await journalService.CreateEntryAsync(request, cancellationToken);
            return Results.Created($"/api/journal/entries/{entry.Id}", entry);
        });

        return app;
    }
}
