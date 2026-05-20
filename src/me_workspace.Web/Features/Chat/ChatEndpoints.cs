namespace me_workspace.Web.Features.Chat;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat");

        group.MapGet("/conversations", async (ChatService chatService, CancellationToken cancellationToken) =>
        {
            var conversations = await chatService.GetConversationsAsync(cancellationToken);
            return Results.Ok(conversations);
        });

        group.MapPost("/conversations", async (CreateConversationRequest? request, ChatService chatService, CancellationToken cancellationToken) =>
        {
            var conversation = await chatService.CreateConversationAsync(request, cancellationToken);
            return Results.Created($"/api/chat/conversations/{conversation.Id}", conversation);
        });

        group.MapGet("/conversations/{conversationId:guid}", async (Guid conversationId, ChatService chatService, CancellationToken cancellationToken) =>
        {
            var conversation = await chatService.GetConversationAsync(conversationId, cancellationToken);
            return conversation is null ? Results.NotFound() : Results.Ok(conversation);
        });

        group.MapPost("/conversations/{conversationId:guid}/messages", async (Guid conversationId, SendMessageRequest request, ChatService chatService, CancellationToken cancellationToken) =>
        {
            var conversation = await chatService.SendMessageAsync(conversationId, request, cancellationToken);
            return conversation is null ? Results.NotFound() : Results.Ok(conversation);
        });

        return app;
    }
}
