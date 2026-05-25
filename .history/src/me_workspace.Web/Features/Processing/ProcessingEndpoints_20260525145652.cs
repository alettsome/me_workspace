using Microsoft.EntityFrameworkCore;

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
        .WithName("ProcessSource");

        app.MapGet("/api/processing/notifications", async (
            Data.AppDbContext db,
            int? limit,
            bool? unreadOnly,
            CancellationToken cancellationToken) =>
        {
            var query = db.ProcessingNotifications.AsQueryable();

            if (unreadOnly == true)
            {
                query = query.Where(n => !n.IsRead);
            }

            query = query.OrderByDescending(n => n.CreatedUtc);

            if (limit.HasValue && limit.Value > 0)
            {
                query = query.Take(limit.Value);
            }
            else
            {
                query = query.Take(50); // Default limit
            }

            var notifications = await query.ToListAsync(cancellationToken);
            return Results.Ok(notifications);
        })
        .WithName("GetNotifications");

        app.MapPost("/api/processing/notifications/{id:guid}/mark-read", async (
            Guid id,
            Data.AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var notification = await db.ProcessingNotifications.FindAsync(new object[] { id }, cancellationToken);
            if (notification == null)
            {
                return Results.NotFound();
            }

            notification.IsRead = true;
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok();
        })
        .WithName("MarkNotificationRead");

        return app;
    }
}
