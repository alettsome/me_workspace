namespace me_workspace.Web.Infrastructure.Llm;

public sealed class LocalLlmClient : ILlmClient
{
    public Task<string> GenerateReplyAsync(string prompt, CancellationToken cancellationToken)
    {
        var userMessage = ReadField(prompt, "USER_MESSAGE");
        var contextSummary = ReadField(prompt, "CONTEXT_SUMMARY");
        var contextSources = ReadField(prompt, "CONTEXT_SOURCES");

        var reply =
            "Local assistant pipeline is connected. " +
            $"I received: \"{userMessage}\". " +
            $"Context used: {contextSources}. " +
            $"{contextSummary}";

        return Task.FromResult(reply.Trim());
    }

    private static string ReadField(string prompt, string fieldName)
    {
        var prefix = $"{fieldName}:";
        var line = prompt
            .Split(Environment.NewLine, StringSplitOptions.None)
            .FirstOrDefault(x => x.StartsWith(prefix, StringComparison.Ordinal));

        return line is null ? string.Empty : line[prefix.Length..].Trim();
    }
}
